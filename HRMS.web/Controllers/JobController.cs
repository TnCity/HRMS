using HRMS.BLL.Services;
using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.web.Controllers
{
    public class JobController : Controller
    {
        private readonly JobService _service;
        private readonly HRMSDbContext _context;

        public JobController(JobService service, HRMSDbContext context)
        {
            _service = service;
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("Admin") != null;
        }

        // ---------------------------------- Show available job -----------------------
        public IActionResult Index()
        {
            var jobs = _service.GetJobs();
            return View(jobs);
        }
        //------------------------------------ Details Vieew -------------------------------

        public IActionResult Details(int id)
        {
            try
            {
                var jobDetails = _context.Jobs
                    .FirstOrDefault( j => j.JobId == id );

                if(jobDetails == null)
                {
                    return NotFound();
                }

                return View(jobDetails);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return View("Error");
        }




        //------------------------------------ Create Job ---------------------------------

        
        public IActionResult Create()
        {
            if(!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Create(Job job)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                _service.AddJob(job);
                return RedirectToAction("Index");
            }
            return View(job);
        }


        // ------------------------------ Edit job ------------------------------
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var job = _service.GetJobById(id);

            if (job == null)
            {
                return NotFound();
            }

            ViewBag.Jobs = _service.GetJobs();

            return View(job);
        }

        [HttpPost]
        public IActionResult Edit(Job job)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                _service.UpdateJob(job);

                return RedirectToAction("Index");
            }

            ViewBag.Jobs = _service.GetJobs();

            return View(job);
        }



        // --------------------------------delete job-----------------------------


        public IActionResult Delete(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var job = _service.GetJobById(id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int jobId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            _service.DeleteJob(jobId);

            return RedirectToAction("Index");
        }


    }
}
