using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HRMSDbContext _context;

        public ReportsController(HRMSDbContext context)
        {
            _context = context;
        }

        // 📊 Main Page (Dropdown load)
        public IActionResult AttendanceReport()
        {
            // 🔒 Admin check
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

        // 📡 API → Get attendance by employee
        public JsonResult GetEmployeeAttendance(int empId)
        {
            var data = _context.Attendances
                .Where(a => a.EmployeeId == empId)
                .AsEnumerable()
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("dd MMM"),
                    present = g.Count(x => x.Status == "Present"),
                    absent = g.Count(x => x.Status == "Absent"),
                    leave = g.Count(x => x.Status == "Leave")
                })
                .OrderBy(x => x.date)
                .ToList();

            return Json(data);
        }

        // 📊 Performance Report Page
        public IActionResult PerformanceReport()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            var data = _context.Performances
                .Include(p => p.Employee) // 🔥 important
                .ToList();

            ViewBag.TotalEmployees = _context.Employees.Count();

            return View(data);
        }

        // 📡 Optional: Performance API (for charts later)
        public JsonResult GetPerformanceData()
        {
            var data = _context.Performances
                .Include(p => p.Employee)
                .Select(p => new
                {
                    name = p.Employee.Name,
                    rating = p.Rating,
                    score = p.ProductivityScore
                })
                .ToList();

            return Json(data);
        }




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
           
                return View(new List<Performance>());

            var data = _context.Performances
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == empId)
                .ToList();

            return View(data);
        }


        public IActionResult ManageReports()
        {
            return View();
        }
    }
}