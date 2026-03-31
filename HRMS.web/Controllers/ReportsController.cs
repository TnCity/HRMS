using HRMS.DAL;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly HRMSDbContext _context;

        public ReportsController(HRMSDbContext context)
        {
            _context = context;
        }

        // 📊 Attendance Report
        public IActionResult AttendanceReport()
        {
            // 🔒 Protect (Admin only)
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login", "Admin");

            var data = _context.Attendances
                .GroupBy(a => a.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Present = g.Count(x => x.Status == "Present"),
                    Absent = g.Count(x => x.Status == "Absent")
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.Dates = data.Select(x => x.Date.ToString("dd-MM")).ToList();
            ViewBag.Present = data.Select(x => x.Present).ToList();
            ViewBag.Absent = data.Select(x => x.Absent).ToList();

            return View();
        }
    }
}