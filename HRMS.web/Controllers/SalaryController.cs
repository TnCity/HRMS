//using HRMS.DAL;
//using HRMS.Entities;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//public class SalaryController : Controller
//{
//    private readonly HRMSDbContext _context;

//    public SalaryController(HRMSDbContext context)
//    {
//        _context = context;
//    }

//    // 🔹 LIST
//    public async Task<IActionResult> Index()
//    {
//        var data = await _context.SalaryStructures
//            .Include(s => s.Employee)
//            .ToListAsync();

//        return View(data);
//    }

//    // 🔹 GET
//    [HttpGet]
//    public IActionResult Create()
//    {
//        var employees = _context.Employees
//            .Select(e => new SelectListItem
//            {
//                Value = e.EmployeeId.ToString(),
//                Text = e.Name
//            })
//            .ToList();

//        var vm = new SalaryVM
//        {
//            Salary = new SalaryStructure(),
//            Employees = employees
//        };

//        return View(vm);
//    }

//    [HttpPost]
//    public async Task<IActionResult> Create(SalaryVM vm)
//    {
//        // 🔥 Force validation re-check
//        ModelState.Remove("Salary.Employee");

//        if (vm.Salary.EmployeeId == 0)
//        {
//            ModelState.AddModelError("Salary.EmployeeId", "Please select employee");
//        }

//        if (ModelState.IsValid)
//        {
//            _context.SalaryStructures.Add(vm.Salary);
//            await _context.SaveChangesAsync();
//            return RedirectToAction("Create");
//        }

//        vm.Employees = _context.Employees
//            .Select(e => new SelectListItem
//            {
//                Value = e.EmployeeId.ToString(),
//                Text = e.Name
//            }).ToList();

//        return View(vm);
//    }
//}


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

    // 🔹 LIST
    public async Task<IActionResult> Index()
    {
        var data = await _context.SalaryStructures
            .Include(s => s.Employee)
            .ToListAsync();

        return View(data);
    }

    // 🔹 GET
    [HttpGet]
    public IActionResult Create()
    {
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

    // 🔥 AUTO-FILL API (IMPORTANT)
    [HttpGet]
    public IActionResult GetEmployeeSalary(int id)
    {
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