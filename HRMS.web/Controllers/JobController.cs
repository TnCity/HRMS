using HRMS.BLL.Services;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.web.Controllers
{
    public class JobController : Controller
    {
        private readonly JobService _service;

        public JobController(JobService service)
        {
            _service = service;
        }

        // ---------------------------------- Show available job -----------------------
        public IActionResult Index()
        {
            var jobs = _service.GetJobs();
            return View(jobs);
        }

        //------------------------------------ Create Job ---------------------------------
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Job job)
        {
            if (ModelState.IsValid)
            {
                _service.AddJob(job);
                return RedirectToAction("Index");
            }
            return View(job);
        }


        // ------------------------------Edit job ------------------------------

        public IActionResult Edit(int id)
        {
            var job = _service.GetJobById(id);

            return View(job);
        }

        [HttpPost]
        public IActionResult Edit(Job job)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateJob(job);

                return RedirectToAction("Index");
            }

            return View(job);
        }



        // --------------------------------delete job-----------------------------


        public IActionResult Delete(int id)
        {
            var job = _service.GetJobById(id);

            return View(job);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int jobId)
        {
            _service.DeleteJob(jobId);

            return RedirectToAction("Index");
        }


    }
}
