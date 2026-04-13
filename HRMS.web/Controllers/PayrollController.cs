using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

public class PayrollController : Controller
{
    private readonly HRMSDbContext _context;

    public PayrollController(HRMSDbContext context)
    {
        _context = context;
    }

    private bool IsAdminLoggedIn()
    {
        return HttpContext.Session.GetString("Admin") != null;
    }

    // 🔹 GET
    public IActionResult Generate()
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        LoadEmployees();

        ViewBag.Month = DateTime.Now.Month;
        ViewBag.Year = DateTime.Now.Year;

        return View();
    }

    // 🔹 POST
    [HttpPost]
    public async Task<IActionResult> Generate(int employeeId, int month, int year)
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        LoadEmployees();

        int yearlyLeaveLimit = 15; // ✅ yearly allowed leave

        // ✅ Salary
        var salary = await _context.SalaryStructures
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

        if (salary == null)
        {
            ViewBag.Error = "Salary structure not found!";
            return View();
        }

        // ✅ Attendance
        var attendance = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                        a.Date.Month == month &&
                        a.Date.Year == year)
            .ToListAsync();

        if (!attendance.Any())
        {
            ViewBag.Error = "No attendance found!";
            return View();
        }

        // ✅ Date ranges
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31);

        // ✅ Working Days
        int totalDays = DateTime.DaysInMonth(year, month);

        int workingDays = Enumerable.Range(1, totalDays)
            .Select(d => new DateTime(year, month, d))
            .Count(d => d.DayOfWeek != DayOfWeek.Saturday &&
                        d.DayOfWeek != DayOfWeek.Sunday);

        // ✅ Present Days
        int presentDays = attendance.Count(a =>
            a.Status.Trim().ToLower() == "present" &&
            a.Date.DayOfWeek != DayOfWeek.Saturday &&
            a.Date.DayOfWeek != DayOfWeek.Sunday);

        // ✅ CURRENT MONTH LEAVES
        var currentMonthLeaves = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                        l.Status == "Approved" &&
                        l.FromDate <= monthEnd &&
                        l.ToDate >= monthStart)
            .ToListAsync();

        int leaveDays = 0;

        foreach (var leave in currentMonthLeaves)
        {
            var start = leave.FromDate < monthStart ? monthStart : leave.FromDate;
            var end = leave.ToDate > monthEnd ? monthEnd : leave.ToDate;

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday)
                {
                    leaveDays++;
                }
            }
        }

        // ✅ TOTAL YEAR LEAVES
        var allYearLeaves = await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId &&
                        l.Status == "Approved" &&
                        l.FromDate <= yearEnd &&
                        l.ToDate >= yearStart)
            .ToListAsync();

        int totalLeaveTaken = 0;

        foreach (var leave in allYearLeaves)
        {
            var start = leave.FromDate < yearStart ? yearStart : leave.FromDate;
            var end = leave.ToDate > yearEnd ? yearEnd : leave.ToDate;

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday)
                {
                    totalLeaveTaken++;
                }
            }
        }

        // ✅ Paid vs Unpaid Leave
        int remainingLeave = yearlyLeaveLimit - (totalLeaveTaken - leaveDays);
        if (remainingLeave < 0) remainingLeave = 0;

        int paidLeave = Math.Min(leaveDays, remainingLeave);
        int unpaidLeave = leaveDays - paidLeave;

        // ✅ Absent Days
        int absentDays = workingDays - (presentDays + paidLeave);
        if (absentDays < 0) absentDays = 0;

        // ✅ Salary
        var gross = Math.Round(
            salary.BasicSalary + salary.HRA + salary.DA + salary.OtherAllowances, 2);

        var perDay = Math.Round(gross / workingDays, 2);

        var lossOfPay = Math.Round((absentDays + unpaidLeave) * perDay, 2);

        var deductions = Math.Round(
            salary.PF + salary.Tax + lossOfPay, 2);

        var net = Math.Round(gross - deductions, 2);

        // ✅ Save
        var payroll = new Payroll
        {
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            TotalDays = workingDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            GrossSalary = gross,
            TotalDeductions = deductions,
            NetSalary = net
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == payroll.Id);

        ViewBag.Result = payroll;

        return View();
    }

    private void LoadEmployees()
    {

        ViewBag.Employees = _context.Employees
            .Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.Name
            }).ToList();
    }

    public async Task<IActionResult> DownloadPayslip(int id)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payroll == null)
            return NotFound();

        // 👉 Get salary structure
        var salary = await _context.SalaryStructures
            .FirstOrDefaultAsync(s => s.EmployeeId == payroll.EmployeeId);

        var vm = new PayslipVM
        {
            Payroll = payroll,
            Salary = salary
        };

        return new ViewAsPdf("PayslipPDF", vm)
        {
            FileName = $"Payslip_{payroll.Employee.Name}_{payroll.Month}_{payroll.Year}.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4
        };
    }



    // for Employee Download PaySlip...
    public async Task<IActionResult> MyPayslips()
    {
        var empId = HttpContext.Session.GetInt32("EmployeeId");

        if (empId == null)
            return RedirectToAction("Login", "Employee");

        var payslips = await _context.Payrolls
            .Where(p => p.EmployeeId == empId)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();

        return View(payslips);
    }
}




