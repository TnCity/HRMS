using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class TaskController : Controller
    {
        private readonly HRMSDbContext _context;

        public TaskController(HRMSDbContext context)
        {
            _context = context;
        }
        public IActionResult AddTask()
        {
            var vm = new AddTaskVM
            {
                Employees = _context.Employees
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = e.Name + " (ID: " + e.EmployeeId + ")"
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddTask(AddTaskVM vm)
        {
            var task = new TaskItem
            {
                EmployeeId = vm.EmployeeId,
                Date = vm.Date,
                IsCompleted = vm.IsCompleted
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return RedirectToAction("AddTask");
        }
    }
}
