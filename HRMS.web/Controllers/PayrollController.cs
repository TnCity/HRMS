using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

    // 🔹 Generate Payroll Page
    public IActionResult Generate()
    {
        if(!IsAdminLoggedIn()) {
            return RedirectToAction("Login", "Admin");
        }

        ViewBag.Employees = _context.Employees
            .Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.Name
            }).ToList();

        ViewBag.Month = DateTime.Now.Month;
        ViewBag.Year = DateTime.Now.Year;

        return View();
    }

    // 🔹 Generate Payroll Logic
    [HttpPost]
    public async Task<IActionResult> Generate(int employeeId, int month, int year)
    {
        if(!IsAdminLoggedIn()) {
            return RedirectToAction("Login", "Admin");
        }
        ViewBag.Employees = _context.Employees
            .Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.Name
            }).ToList();

        var salary = await _context.SalaryStructures
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

        if (salary == null)
        {
            ViewBag.Error = "Salary structure not found!";
            return View();
        }

        var attendance = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId &&
                        a.Date.Month == month &&
                        a.Date.Year == year)
            .ToListAsync();

        int totalDays = DateTime.DaysInMonth(year, month);
        int presentDays = attendance.Count(a => a.Status == "Present");
        int absentDays = totalDays - presentDays;

        var gross = salary.BasicSalary + salary.HRA + salary.DA + salary.OtherAllowances;
        var perDay = gross / totalDays;
        var earned = perDay * presentDays;
        var deductions = salary.PF + salary.Tax + (absentDays * perDay);
        var net = earned - deductions;

        var payroll = new Payroll
        {
            EmployeeId = employeeId,
            Month = month,
            Year = year,
            TotalDays = totalDays,
            PresentDays = presentDays,
            AbsentDays = absentDays,
            GrossSalary = gross,
            TotalDeductions = deductions,
            NetSalary = net
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        // 🔥 send result to UI
        ViewBag.Result = payroll;

        return View();
    }
}