using HRMS.BLL.Services;
using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;


namespace HRMS.web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _service;

        private readonly HRMSDbContext _context;
        public EmployeeController(EmployeeService service, HRMSDbContext context)
        {
            _service = service;
            _context = context;
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

            int pageSize = 5;

            // 🔍 Get all employees first
            var employees = _service.GetEmployees(); // no paging here

            // 🔍 Apply search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                employees = employees.Where(e =>
                    e.Name.ToLower().Contains(search) ||
                    e.Email.ToLower().Contains(search) ||
                    (e.Role != null && e.Role.ToLower().Contains(search))
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

        // ----------------- Details Employee ---------------

        public IActionResult Details(int id)
        {
            var emp = _service.GetEmployeeById(id);
            if (emp == null)
            {
                return NotFound();

            }
            return View(emp);
        }


        //  -------------------------- Edit Employee-----------------------------

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

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                existingEmp.ProfileImagePath = "/images/" + fileName;
            }

            _service.UpdateEmployee(emp);

            return RedirectToAction("Index");
        }

        // ------------------------- Delete Employee // ----------------------------

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



        // ------------------------- Login Employee ----------------------------
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _service.Login(model.Email, model.Password);

            if (user != null)
            {
                // First-time login check
                if (user.IsFirstLogin)
                {
                    return RedirectToAction("ChangePassword", new { id = user.EmployeeId });
                }

                // Store session
                HttpContext.Session.SetInt32("EmployeeId", user.EmployeeId);
                HttpContext.Session.SetString("EmployeeName", user.Name);

                return RedirectToAction("Dashboard", "Employee");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        // ----------------------------Employee Logout:-----------------------------

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");

        }

        // -------------------------Employee Change Password ----------------------------
        public IActionResult ChangePassword(int Id)
        {
            var model = new ChangePasswordVM
            {
                EmployeeId = Id
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var emp = _service.GetEmployeeById(model.EmployeeId);
            if (emp == null)
            {
                return NotFound();
            }
            emp.Password = model.NewPassword;
            emp.IsFirstLogin = false;
            _service.UpdateEmployee(emp);
            TempData["success"] = "Password changed successfully! Please log in with your new password.";
            return RedirectToAction("Login");

        }

        //------------------------ Employee Dashboard.--------------------------
        public IActionResult Dashboard()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
            {
                return RedirectToAction("Login");
            }

            var emp = _service.GetEmployeeById(empId.Value);

            return View(emp);
        }

        // Employee can see there own Attendence //---------------------------

        //public async Task<IActionResult> MyAttendance()
        //{
        //    var empId = HttpContext.Session.GetInt32("EmployeeId");

        //    if (empId == null)
        //        return RedirectToAction("Login", "Employee");

        //    var attendance = await _context.Attendances
        //        .Where(a => a.EmployeeId == empId)
        //        .OrderByDescending(a => a.Date)
        //        .ToListAsync();

        //    return View(attendance);
        //}


        //--------------------test----------------
        public async Task<IActionResult> MyAttendance()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Employee");

            // 🔹 Get employee code
            var empCode = await _context.Employees
                .Where(e => e.EmployeeId == empId)
                .Select(e => e.EmployeeCode)
                .FirstOrDefaultAsync();

            // 🔹 Get attendance summary
            var attendance = await _context.Attendances
                .Where(a => a.EmployeeId == empId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // 🔹 Get raw punch logs
            var rawLogs = await _context.AttendanceRawDatas
                .Where(r => r.EmployeeCode == empCode)
                .ToListAsync();

            // 🔹 Group punches by date (for easy UI)
            var rawGrouped = rawLogs
                .GroupBy(r => r.Timestamp.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Timestamp)
                          .Select(x => x.Timestamp.ToString("HH:mm"))
                          .ToList()
                );

            // 🔹 Combine everything
            var result = attendance.Select(a => new AttendanceDetailVM
            {
                Date = a.Date,
                Status = a.Status,
                Login = a.LoginTime,
                Logout = a.LogoutTime,
                TotalHours = a.TotalHours,
                WorkingHours = a.WorkingHours,

                Punches = rawGrouped.ContainsKey(a.Date.Date)
        ? rawGrouped[a.Date.Date]
        : new List<string>()
            }).ToList();

            return View(result);
        }
    }
}
