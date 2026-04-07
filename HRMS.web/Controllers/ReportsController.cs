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

        public ReportsController(HRMSDbContext context, PerformanceService service)
        {
            _context = context;
            _service = service;
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


        // 📡 Attendance API
        public JsonResult GetEmployeeAttendance(int empId)
        {
            var data = _context.Attendances
                .Where(a => a.EmployeeId == empId)
                .AsEnumerable()
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

            return Json(data);
        }

        // 📊 Performance Report (REAL-TIME)
        public IActionResult PerformanceReport()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            // ✅ Approved leaves
            var leaveData = _context.LeaveRequests
                .Where(l => l.Status == "Approved")
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            // ✅ Build data
            var data = _context.Employees
                .Select(e => new PerformanceVM
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.Name,
                    TasksCompleted = 10, // temporary
                    LeavesTaken = leaveData.ContainsKey(e.EmployeeId)
                                    ? leaveData[e.EmployeeId]
                                    : 0
                })
                .ToList();

            // ✅ Calculate performance
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

            // ✅ Dashboard cards
            ViewBag.TotalEmployees = data.Count;
            ViewBag.TopPerformers = data.Count(x => x.Rating >= 4);
            ViewBag.Average = data.Count(x => x.Rating == 3);
            ViewBag.LowPerformers = data.Count(x => x.Rating <= 2);

            return View(data);
        }

        // 📡 Performance API (for charts)
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

            // 🔹 Dropdown
            ViewBag.Employees = _context.Employees
                .Select(e => new
                {
                    Id = e.EmployeeId,
                    Name = e.Name
                }).ToList();

            if (empId == null)
                return View(new List<PerformanceVM>());

            // 🔥 STEP 1: Generate latest monthly data
            _service.GenerateMonthlyPerformance(DateTime.Now.Year, DateTime.Now.Month);

            // 🔥 STEP 2: Fetch monthly performance (REAL DATA)
            var monthlyData = _context.MonthlyPerformances
                .Where(x => x.EmployeeId == empId)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            ViewBag.MonthlyData = monthlyData;

            // 🔥 STEP 3: Latest summary (for table)
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
    }
}