using HRMS.BLL.Services;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _service;

        public EmployeeController(EmployeeService service)
        {
            _service = service;
        }

        // 🔒 Login Check   <<- its controll on for Hr
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("Admin") != null;
        }

        // ✅ INDEX
        public IActionResult Index(string search, int page = 1)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "Admin");

            int pageSize = 6;

            // 🔍 Get all employees first
            var employees = _service.GetEmployees(); // no paging here

            // 🔍 Apply search
            if (!string.IsNullOrEmpty(search))
            {
                employees = employees.Where(e =>
                    e.Name.Contains(search) ||
                    e.Email.Contains(search) ||
                    (e.Role != null && e.Role.Contains(search))
                ).ToList();
            }

            int totalRecords = employees.Count();

            // 📄 Apply pagination AFTER search
            var data = employees
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;

            return View(data);
        }

        //  --------------------------Create Employee (HR can access)-----------------------------

        public IActionResult Create()
        {
            var departments = _service.GetDepartments();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

           return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee emp, IFormFile ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                var departments = _service.GetDepartments();
                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", emp.DepartmentId);
                return View(emp);
            }

            // ✅ Image Upload
            if (ProfileImage != null)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                emp.ProfileImagePath = "/images/" + fileName;
            }
            

            _service.AddEmployee(emp);

            TempData["success"] = "Employee added successfully!";
            return RedirectToAction(nameof(Index));
        }

        //  --------------------------Edit Employee-----------------------------

        public IActionResult Edit(int id)
        {
            var emp = _service.GetEmployeeById(id);

            var departments = _service.GetDepartments();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", emp.DepartmentId);

            return View(emp);
        }


        [HttpPost]
        public IActionResult Edit(Employee emp, IFormFile? ProfileImage)
        {
            var existingEmp = _service.GetEmployeeById(emp.EmployeeId);

            if (!ModelState.IsValid)
            {
                var departments = _service.GetDepartments();
                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", emp.DepartmentId);
                return View(emp);
            }

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                emp.ProfileImagePath = "/images/" + fileName;
            }
            else
            {
                emp.ProfileImagePath = existingEmp.ProfileImagePath;
            }

            _service.UpdateEmployee(emp);

            return RedirectToAction("Index");
        }

        // ------------------------- Delete Employee ----------------------------

        public IActionResult Delete(int id)
        {
            var emp = _service.GetEmployeeById(id);
            return View(emp);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _service.DeleteEmployee(id);
            return RedirectToAction("Index");
        }

    }
}
