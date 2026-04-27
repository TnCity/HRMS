//using HRMS.BLL.Services;
//using HRMS.DAL;
//using HRMS.web.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.web.Controllers
//{
//    public class ReportsController : Controller
//    {
//        private readonly HRMSDbContext _context;
//        private readonly PerformanceService _service;
//        private readonly AttendanceService _attendanceService;

//        public ReportsController(
//            HRMSDbContext context,
//            PerformanceService service,
//            AttendanceService attendanceService)
//        {
//            _context = context;
//            _service = service;
//            _attendanceService = attendanceService;
//        }

//        // ===================== DASHBOARD =====================
//        public IActionResult ManageReports()
//        {
//            return View();
//        }

//        // ===================== ATTENDANCE =====================
//        public IActionResult AttendanceReport()
//        {
//            if (HttpContext.Session.GetString("Admin") == null)
//                return RedirectToAction("Login", "Admin");

//            ViewBag.Employees = _context.Employees
//                .Select(e => new
//                {
//                    Id = e.EmployeeId,
//                    Name = e.Name
//                }).ToList();

//            return View();
//        }

//        public async Task<JsonResult> GetEmployeeAttendance(int empId, int year, int month)
//        {
//            // ✅ ALWAYS process latest raw data
//            await _attendanceService.ProcessAttendanceAsync();

//            var start = new DateTime(year, month, 1);
//            var end = start.AddMonths(1);

//            var data = await _context.Attendances
//                .Where(a => a.EmployeeId == empId &&
//                            a.Date >= start &&
//                            a.Date < end)
//                .ToListAsync();

//            // ✅ Daily grouping
//            var result = data
//                .GroupBy(a => a.Date.Date)
//                .Select(g => new
//                {
//                    rawDate = g.Key,
//                    date = g.Key.ToString("dd MMM"),

//                    present = g.Count(x => x.Status == "Present"),
//                    absent = g.Count(x => x.Status == "Absent"),
//                    leave = g.Count(x => x.Status == "Leave"),
//                    incomplete = g.Count(x => x.Status == "Incomplete") // ✅ FIXED
//                })
//                .OrderBy(x => x.rawDate)
//                .ToList();

//            return Json(result);
//        }

//        // ===================== PERFORMANCE =====================
//        public async Task<IActionResult> PerformanceReport(int? year, int? month)
//        {
//            if (HttpContext.Session.GetString("Admin") == null)
//                return RedirectToAction("Login", "Admin");

//            int y = year ?? DateTime.Now.Year;
//            int m = month ?? DateTime.Now.Month;

//            if (m < 1 || m > 12)
//                m = DateTime.Now.Month;

//            // ✅ Generate data first
//            await _service.GenerateMonthlyPerformance(y, m);

//            var data = await _context.MonthlyPerformances
//                .Include(x => x.Employee)
//                .Where(x => x.Year == y && x.Month == m)
//                .ToListAsync();

//            var result = data.Select(x =>
//            {
//                int rating = x.Score switch
//                {
//                    >= 80 => 5,
//                    >= 60 => 4,
//                    >= 40 => 3,
//                    >= 25 => 2,
//                    _ => 1
//                };

//                string status = rating switch
//                {
//                    >= 4 => "Good",
//                    3 => "Average",
//                    _ => "Poor"
//                };

//                return new PerformanceVM
//                {
//                    EmployeeId = x.EmployeeId,
//                    EmployeeName = x.Employee.Name,
//                    TasksCompleted = x.TasksCompleted,
//                    LeavesTaken = x.LeavesTaken,
//                    ProductivityScore = x.Score,
//                    Rating = rating,
//                    Status = status
//                };
//            }).ToList();

//            ViewBag.Year = y;
//            ViewBag.Month = m;

//            ViewBag.TotalEmployees = result.Count;
//            ViewBag.TopPerformers = result.Count(x => x.Rating >= 4);
//            ViewBag.Average = result.Count(x => x.Rating == 3);
//            ViewBag.LowPerformers = result.Count(x => x.Rating <= 2);

//            return View(result);
//        }

//        public async Task<JsonResult> GetPerformanceData(int? year, int? month)
//        {
//            int y = year ?? DateTime.Now.Year;
//            int m = month ?? DateTime.Now.Month;

//            if (m < 1 || m > 12)
//                m = DateTime.Now.Month;

//            await _service.GenerateMonthlyPerformance(y, m);

//            var data = await _context.MonthlyPerformances
//                .Include(x => x.Employee)
//                .Where(x => x.Year == y && x.Month == m)
//                .ToListAsync();

//            var result = data.Select(x => new
//            {
//                name = x.Employee.Name,
//                score = x.Score
//            });

//            return Json(result);
//        }

//        // ===================== INDIVIDUAL =====================
//        public async Task<IActionResult> IndividualPerformance(int? empId)
//        {
//            if (HttpContext.Session.GetString("Admin") == null)
//                return RedirectToAction("Login", "Admin");

//            ViewBag.Employees = await _context.Employees
//                .Select(e => new
//                {
//                    Id = e.EmployeeId,
//                    Name = e.Name
//                }).ToListAsync();

//            if (empId == null)
//                return View(new List<PerformanceVM>());

//            int year = DateTime.Now.Year;
//            int currentMonth = DateTime.Now.Month;

//            // Generate last 3 months
//            for (int m = currentMonth - 3; m <= currentMonth; m++)
//            {
//                if (m > 0)
//                    await _service.GenerateMonthlyPerformance(year, m);
//            }

//            var monthlyData = await _context.MonthlyPerformances
//                .Where(x => x.EmployeeId == empId)
//                .OrderBy(x => x.Year)
//                .ThenBy(x => x.Month)
//                .ToListAsync();

//            ViewBag.MonthlyData = monthlyData;

//            var latest = monthlyData.LastOrDefault();
//            if (latest == null)
//                return View(new List<PerformanceVM>());

//            var emp = await _context.Employees
//                .FirstOrDefaultAsync(e => e.EmployeeId == empId);

//            int rating = latest.Score switch
//            {
//                >= 80 => 5,
//                >= 60 => 4,
//                >= 40 => 3,
//                >= 25 => 2,
//                _ => 1
//            };

//            string status = rating switch
//            {
//                >= 4 => "Good",
//                3 => "Average",
//                _ => "Poor"
//            };

//            var vm = new PerformanceVM
//            {
//                EmployeeId = emp.EmployeeId,
//                EmployeeName = emp.Name,
//                TasksCompleted = latest.TasksCompleted,
//                LeavesTaken = latest.LeavesTaken,
//                ProductivityScore = latest.Score,
//                Rating = rating,
//                Status = status
//            };

//            return View(new List<PerformanceVM> { vm });
//        }

//        // for specefic reports...oa an Employee.

//        public async Task<JsonResult> GetAttendanceWithDetails(int empId, int year, int month)
//        {
//            var start = new DateTime(year, month, 1);
//            var end = start.AddMonths(1);

//            var empCode = await _context.Employees
//                .Where(e => e.EmployeeId == empId)
//                .Select(e => e.EmployeeCode)
//                .FirstOrDefaultAsync();

//            if (empCode == 0)
//                return Json(new List<object>());

//            var attendance = await _context.Attendances
//                .Where(a => a.EmployeeId == empId &&
//                            a.Date >= start && a.Date < end)
//                .ToListAsync();

//            var rawLogs = await _context.AttendanceRawDatas
//                .Where(r => r.EmployeeCode == empCode.ToString() &&
//                            r.Timestamp >= start &&
//                            r.Timestamp < end)
//                .ToListAsync();

//            var result = attendance.Select(a => new
//            {
//                date = a.Date.ToString("dd MMM"),
//                rawDate = a.Date,

//                status = a.Status,
//                login = a.LoginTime,
//                logout = a.LogoutTime,
//                totalHours = a.TotalHours,
//                workingHours = a.WorkingHours,

//                punches = rawLogs
//                    .Where(r => r.Timestamp.Date == a.Date.Date)
//                    .Select(r => r.Timestamp.ToString("HH:mm"))
//                    .ToList()
//            });

//            return Json(result);
//        }
//    }
//}


using HRMS.BLL.Services;
using HRMS.DAL;
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

        // ===================== DASHBOARD =====================
        public IActionResult ManageReports()
        {
            return View();
        }

        // ===================== ATTENDANCE =====================
        public IActionResult AttendanceReport()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            ViewBag.Employees = _context.Employees
                .Select(e => new
                {
                    Id = e.EmployeeId,
                    Name = e.Name
                }).ToList();

            return View();
        }

        public async Task<JsonResult> GetEmployeeAttendance(int empId, int year, int month)
        {
            await _attendanceService.ProcessAttendanceAsync();

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var data = await _context.Attendances
                .Where(a => a.EmployeeId == empId &&
                            a.Date >= start &&
                            a.Date < end)
                .ToListAsync();

            var result = data
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    rawDate = g.Key,
                    date = g.Key.ToString("dd MMM"),

                    present = g.Count(x => x.Status == "Present"),
                    absent = g.Count(x => x.Status == "Absent"),
                    leave = g.Count(x => x.Status == "Leave"),
                    incomplete = g.Count(x => x.Status == "Incomplete")
                })
                .OrderBy(x => x.rawDate)
                .ToList();

            return Json(result);
        }

        // ===================== ATTENDANCE DETAILS =====================
        public async Task<JsonResult> GetAttendanceWithDetails(int empId, int year, int month)
        {
            await _attendanceService.ProcessAttendanceAsync();

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var empCode = await _context.Employees
                .Where(e => e.EmployeeId == empId)
                .Select(e => e.EmployeeCode)
                .FirstOrDefaultAsync();

            if (empCode == 0)
                return Json(new List<object>());

            var attendance = await _context.Attendances
                .Where(a => a.EmployeeId == empId &&
                            a.Date >= start && a.Date < end)
                .ToListAsync();

            var rawLogs = await _context.AttendanceRawDatas
                .Where(r => r.EmployeeCode == empCode &&
                            r.Timestamp >= start &&
                            r.Timestamp < end)
                .ToListAsync();

            // 🔥 Group once (performance optimized)
            var rawGrouped = rawLogs
                .GroupBy(r => r.Timestamp.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Timestamp)
                          .Select(x => x.Timestamp.ToString("HH:mm"))
                          .ToList()
                );

            var result = attendance.Select(a => new
            {
                date = a.Date.ToString("dd MMM"),
                rawDate = a.Date,

                status = a.Status,
                login = a.LoginTime,
                logout = a.LogoutTime,
                totalHours = a.TotalHours,
                workingHours = a.WorkingHours,

                punches = rawGrouped.ContainsKey(a.Date.Date)
                    ? rawGrouped[a.Date.Date]
                    : new List<string>()
            })
            .OrderBy(x => x.rawDate)
            .ToList();

            return Json(result);
        }

        // ===================== PERFORMANCE =====================
        public async Task<IActionResult> PerformanceReport(int? year, int? month)
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            if (m < 1 || m > 12)
                m = DateTime.Now.Month;

            await _service.GenerateMonthlyPerformance(y, m);

            var data = await _context.MonthlyPerformances
                .Include(x => x.Employee)
                .Where(x => x.Year == y && x.Month == m)
                .ToListAsync();

            var result = data.Select(x =>
            {
                int rating = x.Score switch
                {
                    >= 80 => 5,
                    >= 60 => 4,
                    >= 40 => 3,
                    >= 25 => 2,
                    _ => 1
                };

                string status = rating switch
                {
                    >= 4 => "Good",
                    3 => "Average",
                    _ => "Poor"
                };

                return new PerformanceVM
                {
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee.Name,
                    TasksCompleted = x.TasksCompleted,
                    LeavesTaken = x.LeavesTaken,
                    ProductivityScore = x.Score,
                    Rating = rating,
                    Status = status
                };
            }).ToList();

            ViewBag.Year = y;
            ViewBag.Month = m;

            ViewBag.TotalEmployees = result.Count;
            ViewBag.TopPerformers = result.Count(x => x.Rating >= 4);
            ViewBag.Average = result.Count(x => x.Rating == 3);
            ViewBag.LowPerformers = result.Count(x => x.Rating <= 2);

            return View(result);
        }

        public async Task<JsonResult> GetPerformanceData(int? year, int? month)
        {
            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            if (m < 1 || m > 12)
                m = DateTime.Now.Month;

            await _service.GenerateMonthlyPerformance(y, m);

            var data = await _context.MonthlyPerformances
                .Include(x => x.Employee)
                .Where(x => x.Year == y && x.Month == m)
                .ToListAsync();

            var result = data.Select(x => new
            {
                name = x.Employee.Name,
                score = x.Score
            });

            return Json(result);
        }

        // ===================== INDIVIDUAL PERFORMANCE =====================
        public async Task<IActionResult> IndividualPerformance(int? empId)
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            ViewBag.Employees = await _context.Employees
                .Select(e => new
                {
                    Id = e.EmployeeId,
                    Name = e.Name
                }).ToListAsync();

            if (empId == null)
                return View(new List<PerformanceVM>());

            int year = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            for (int m = currentMonth - 3; m <= currentMonth; m++)
            {
                if (m > 0)
                    await _service.GenerateMonthlyPerformance(year, m);
            }

            var monthlyData = await _context.MonthlyPerformances
                .Where(x => x.EmployeeId == empId)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            ViewBag.MonthlyData = monthlyData;

            var latest = monthlyData.LastOrDefault();
            if (latest == null)
                return View(new List<PerformanceVM>());

            var emp = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == empId);

            int rating = latest.Score switch
            {
                >= 80 => 5,
                >= 60 => 4,
                >= 40 => 3,
                >= 25 => 2,
                _ => 1
            };

            string status = rating switch
            {
                >= 4 => "Good",
                3 => "Average",
                _ => "Poor"
            };

            var vm = new PerformanceVM
            {
                EmployeeId = emp.EmployeeId,
                EmployeeName = emp.Name,
                TasksCompleted = latest.TasksCompleted,
                LeavesTaken = latest.LeavesTaken,
                ProductivityScore = latest.Score,
                Rating = rating,
                Status = status
            };

            return View(new List<PerformanceVM> { vm });
        }
    }
}