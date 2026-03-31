using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly HRMSDbContext _context;

        public DepartmentController(HRMSDbContext context)
        {
            _context = context;
        }

        // 🔹 INDEX (List)
        public IActionResult Index()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }

        // 🔹 CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 CREATE (POST)
        [HttpPost]
        public IActionResult Create(Department dept)
        {
            if (!ModelState.IsValid)
                return View(dept);

            _context.Departments.Add(dept);
            _context.SaveChanges();

            TempData["success"] = "Department added successfully!";
            return RedirectToAction("Index");
        }

        // 🔹 EDIT (GET)
        public IActionResult Edit(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            return View(dept);
        }

        // 🔹 EDIT (POST)
        [HttpPost]
        public IActionResult Edit(Department dept)
        {
            if (!ModelState.IsValid)
                return View(dept);

            _context.Departments.Update(dept);
            _context.SaveChanges();

            TempData["success"] = "Department updated successfully!";
            return RedirectToAction("Index");
        }

        // 🔹 DELETE (GET)
        public IActionResult Delete(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            return View(dept);
        }

        // 🔹 DELETE (POST)
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept != null)
            {
                _context.Departments.Remove(dept);
                _context.SaveChanges();
            }

            TempData["success"] = "Department deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}