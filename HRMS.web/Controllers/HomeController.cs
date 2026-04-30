using System.Diagnostics;
using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HRMSDbContext _context;

        public HomeController(ILogger<HomeController> logger, HRMSDbContext context)
        {
            _logger = logger;
            _context = context;
        }
            
        public IActionResult Index()
        {
            return View();
        }

        //----------------------------------------------------

        public IActionResult About()
        {
            return View();
        }


        //----------------------------------------------------
        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;

                _context.ContactMessages.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Message sent successfully!";
                return RedirectToAction("Contact");
            }

            return View(model);
        }
        //---------------------------------------------------------


        public IActionResult Privacy()
        {
            return View();
        }


        //----------------------------------------------------------


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
