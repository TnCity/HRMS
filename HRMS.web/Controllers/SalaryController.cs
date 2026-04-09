

using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class SalaryController : Controller
{
    private readonly HRMSDbContext _context;

    public SalaryController(HRMSDbContext context)
    {
        _context = context;
    }
    private bool IsAdminLoggedIn()
    {
        return HttpContext.Session.GetString("Admin") != null;
    }

    // 🔹 LIST
    public async Task<IActionResult> Index()
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        var data = await _context.SalaryStructures
            .Include(s => s.Employee)
            .ToListAsync();

        return View(data);
    }
    
    // 🔹 GET
    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        var vm = new SalaryVM
        {
            Salary = new SalaryStructure(),
            Employees = _context.Employees
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.Name
                }).ToList()
        };

        return View(vm);
    }

    // 🔹 POST
    [HttpPost]
    public async Task<IActionResult> Create(SalaryVM vm)
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        ModelState.Remove("Salary.Employee");

        if (vm.Salary.EmployeeId == 0)
        {
            ModelState.AddModelError("Salary.EmployeeId", "Please select employee");
        }

        if (ModelState.IsValid)
        {
            _context.SalaryStructures.Add(vm.Salary);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create");
        }

        vm.Employees = _context.Employees
            .Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.Name
            }).ToList();

        return View(vm);
    }

    //  AUTO-FILL API (IMPORTANT)
    [HttpGet]
    public IActionResult GetEmployeeSalary(int id)
    {
        if (!IsAdminLoggedIn())
            return RedirectToAction("Login", "Admin");

        var emp = _context.Employees
            .Where(e => e.EmployeeId == id)
            .Select(e => new
            {
                salary = e.Salary
            })
            .FirstOrDefault();

        if (emp == null)
            return NotFound();

        return Json(emp);
    }
}