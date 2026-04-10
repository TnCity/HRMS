using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace HRMS.web.Controllers
{
    public class LeaveController : Controller
    {
        private readonly HRMSDbContext _context;

        public LeaveController(HRMSDbContext context)
        {
            _context = context;
        }
        public IActionResult Apply()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Apply(LeaveRequest leave)
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Login", "Employee");
            }

            if (leave.FromDate > leave.ToDate)
            {
                ModelState.AddModelError("", "From Date cannot be later than To Date.");
            }

            if (!ModelState.IsValid)
            {
                return View(leave);
            }

            leave.EmployeeId = employeeId.Value; 
            leave.Status = "Pending";
            leave.AppliedOn = DateTime.Now;

            _context.LeaveRequests.Add(leave);
            _context.SaveChanges();

            TempData["success"] = "Leave applied successfully!";

            return RedirectToAction("MyLeaves");
        }
        public IActionResult MyLeaves()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Employee");

            var leaves = _context.LeaveRequests
                .Where(l => l.EmployeeId == empId)
                .ToList();

            return View(leaves);
        }

        //-------------------------------------- ✅ ADMIN VIEW -----------------------------------
        public IActionResult Manage()
        {
            var leaves = _context.LeaveRequests
                .Include(l => l.Employee)
                .ToList();

            return View(leaves);
        }

        // ✅ APPROVE
        public IActionResult Approve(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            if (leave != null)
            {
                leave.Status = "Approved";
                _context.SaveChanges();
            }

            return RedirectToAction("Manage");
        }

        // ✅ REJECT
        public IActionResult Reject(int id)
        {
            var leave = _context.LeaveRequests.Find(id);

            
            if (leave != null)
            {
                leave.Status = "Rejected";
                _context.SaveChanges();
            }

            return RedirectToAction("Manage");
        }
    }
}
