


using HRMS.DAL;
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
                .OrderBy(a => a.Date)
                .Select(a => new
                {
                    date = a.Date.ToString("dd-MM"),
                    status = a.Status == "Present" ? 1 : 0
                })
                .ToList();

            return Json(data);
        }

        public IActionResult ManageReports()
        {
            return View();
        }
    }
}