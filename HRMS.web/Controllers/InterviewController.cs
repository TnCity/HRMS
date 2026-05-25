using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class InterviewController : Controller
    {
        private readonly HRMSDbContext _context;

        public InterviewController(HRMSDbContext context)
        {
            _context = context;
        }

        // ================= CHECK ADMIN LOGIN =================

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("Admin") != null;
        }

        // ================= SCHEDULE INTERVIEW =================

        [HttpGet]
        public IActionResult Create(int applicationId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var interview = new Interview
            {
                ApplicationId = applicationId,
                InterviewDate = DateTime.Now
            };

            return View(interview);
        }

        [HttpPost]
        public IActionResult Create(Interview model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Interviews.Add(model);

            _context.SaveChanges();

            TempData["Success"] =
                "Interview scheduled successfully.";

            return RedirectToAction(
                "AppliedJob",
                "Job");
        }

        // ================= INTERVIEW RESULT =================

        [HttpGet]
        public IActionResult InterviewResult(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var interview = _context.Interviews
                .Include(i => i.Application)
                .FirstOrDefault(i => i.InterviewId == id);

            if (interview == null)
                return NotFound();

            var result = new InterviewResult
            {
                InterviewId = interview.InterviewId
            };

            return View(result);
        }

        [HttpPost]
        public IActionResult InterviewResult(InterviewResult model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.InterviewResults.Add(model);

            // UPDATE APPLICATION STATUS

            var interview = _context.Interviews
                .FirstOrDefault(i => i.InterviewId == model.InterviewId);

            if (interview != null)
            {
                var application = _context.Applications
                    .FirstOrDefault(a =>
                        a.ApplicationId == interview.ApplicationId);

                if (application != null)
                {
                    application.Status = model.Result;
                }
            }

            _context.SaveChanges();

            TempData["Success"] =
                "Interview result added successfully.";

            return RedirectToAction("AppliedJob", "Job");
        }

        //-----------------------------------------Interview Status.---------------------------------------------


        public IActionResult InterviewStatus()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var interviews = _context.InterviewResults
                .Include(r => r.Interview)
                .ThenInclude(i => i.Application)
                .ThenInclude(a => a.Candidate)
                .Include(r => r.Interview.Application.Job)
                .ToList();

            return View(interviews);
        }
    }
}