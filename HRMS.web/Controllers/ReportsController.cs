
using HRMS.BLL.Services;
using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HRMSDbContext _context;
        private readonly PerformanceService _service;
        private readonly AttendanceService _attendanceService;

        public ReportsController(
            HRMSDbContext context,
            PerformanceService service,
            AttendanceService attendanceService)
        {
            _context = context;
            _service = service;
            _attendanceService = attendanceService;
        }

        // 📊 Attendance Report Page
        public IActionResult AttendanceReport()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            var employees = _context.Employees
                .Select(e => new
                {
                    Id = e.EmployeeId,
                    Name = e.Name
                })
                .ToList();

            ViewBag.Employees = employees;

            return View();
        }

        // 📡 Attendance API (FIXED)
        public async Task<JsonResult> GetEmployeeAttendance(int empId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var data = await _context.Attendances
                .Where(a => a.EmployeeId == empId &&
                            a.Date >= startDate &&
                            a.Date < endDate)
                .ToListAsync();

            var result = data
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    rawDate = g.Key,
                    date = g.Key.ToString("dd MMM"),
                    present = g.Count(x => x.Status == "Present"),
                    absent = g.Count(x => x.Status == "Absent"),
                    leave = g.Count(x => x.Status == "Leave")
                })
                .OrderBy(x => x.rawDate)
                .ToList();

            return Json(result);
        }

        // 📊 Performance Report (REAL-TIME)
        public IActionResult PerformanceReport()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            var leaveData = _context.LeaveRequests
                .Where(l => l.Status == "Approved")
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var data = _context.Employees
                .Select(e => new PerformanceVM
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.Name,
                    TasksCompleted = 10,
                    LeavesTaken = leaveData.ContainsKey(e.EmployeeId)
                                    ? leaveData[e.EmployeeId]
                                    : 0
                })
                .ToList();

            foreach (var item in data)
            {
                item.ProductivityScore = (item.TasksCompleted * 5) - (item.LeavesTaken * 2);

                item.Rating = item.ProductivityScore switch
                {
                    >= 80 => 5,
                    >= 60 => 4,
                    >= 40 => 3,
                    >= 25 => 2,
                    _ => 1
                };

                item.Status = item.Rating switch
                {
                    >= 4 => "Good",
                    3 => "Average",
                    _ => "Poor"
                };
            }

            ViewBag.TotalEmployees = data.Count;
            ViewBag.TopPerformers = data.Count(x => x.Rating >= 4);
            ViewBag.Average = data.Count(x => x.Rating == 3);
            ViewBag.LowPerformers = data.Count(x => x.Rating <= 2);

            return View(data);
        }

        // 📊 Performance API
        public JsonResult GetPerformanceData()
        {
            var leaveData = _context.LeaveRequests
                .Where(l => l.Status == "Approved")
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var data = _context.Employees
                .Select(e => new
                {
                    name = e.Name,
                    tasks = 10,
                    leaves = leaveData.ContainsKey(e.EmployeeId)
                                ? leaveData[e.EmployeeId]
                                : 0
                })
                .AsEnumerable()
                .Select(x => new
                {
                    name = x.name,
                    score = (x.tasks * 5) - (x.leaves * 2),
                    rating = ((x.tasks * 5) - (x.leaves * 2)) switch
                    {
                        >= 80 => 5,
                        >= 60 => 4,
                        >= 40 => 3,
                        >= 25 => 2,
                        _ => 1
                    }
                })
                .ToList();

            return Json(data);
        }

        // 👤 Individual Performance
        public IActionResult IndividualPerformance(int? empId)
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            ViewBag.Employees = _context.Employees
                .Select(e => new
                {
                    Id = e.EmployeeId,
                    Name = e.Name
                }).ToList();

            if (empId == null)
                return View(new List<PerformanceVM>());

            _service.GenerateMonthlyPerformance(DateTime.Now.Year, DateTime.Now.Month);

            var monthlyData = _context.MonthlyPerformances
                .Where(x => x.EmployeeId == empId)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            ViewBag.MonthlyData = monthlyData;

            var latest = monthlyData.LastOrDefault();
            var data = new List<PerformanceVM>();

            if (latest != null)
            {
                var emp = _context.Employees.FirstOrDefault(e => e.EmployeeId == empId);
                if (emp == null)
                    return View(new List<PerformanceVM>());

                var vm = new PerformanceVM
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeName = emp.Name,
                    TasksCompleted = latest.TasksCompleted,
                    LeavesTaken = latest.LeavesTaken,
                    ProductivityScore = latest.Score
                };

                vm.Rating = vm.ProductivityScore switch
                {
                    >= 90 => 5,
                    >= 70 => 4,
                    >= 50 => 3,
                    >= 30 => 2,
                    _ => 1
                };

                vm.Status = vm.Rating switch
                {
                    >= 4 => "Good",
                    3 => "Average",
                    _ => "Poor"
                };

                data.Add(vm);
            }

            return View(data);
        }

        public IActionResult ManageReports()
        {
            return View();
        }

        // ============================================================================
        public async Task<IActionResult> TestGenerateAll()
        {
            var empIds = _context.AttendanceLogs
                .Select(x => x.EmployeeId)
                .Distinct()
                .ToList();

            foreach (var empId in empIds)
            {
                var dates = _context.AttendanceLogs
                    .Where(x => x.EmployeeId == empId)
                    .Select(x => x.TimeStamp.Date)
                    .Distinct()
                    .ToList();

                foreach (var date in dates)
                {
                    await _attendanceService.GenerateAttendanceForDay(empId, date);
                }

                var allMonths = _context.AttendanceLogs
                .Where(x => x.EmployeeId == empId)
                .Select(x => new { x.TimeStamp.Year, x.TimeStamp.Month })
                .Distinct()
                .ToList();

                foreach (var m in allMonths)
                {
                    await _attendanceService.GenerateAbsentForMonth(empId, m.Year, m.Month);
                }
            }

            return Content("All Employees Processed");
        }
    }
}