using System;
using System.Linq;
using HRMS.DAL;
using HRMS.Entities;
using HRMS.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly HRMSDbContext _context;

        public ApplicationsController(HRMSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Apply(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.JobId == id);
            if (job == null) return NotFound();

            if (job.LastDate < DateTime.UtcNow)
            {
                TempData["Error"] = "The application period for this job has expired.";
                return View("Expired", job);
            }

            ViewData["Job"] = job;
            return View(new ApplicationVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Apply(int id, ApplicationVM model)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.JobId == id);

            if (job == null)
                return NotFound();

            if (job.LastDate < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty,
                    "Application has expired.");

                ViewData["Job"] = job;

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewData["Job"] = job;

                return View(model);
            }

            var alreadyApplied = _context.Applications
                .Include(a => a.Candidate)
                .Any(a => a.JobId == id &&
                          a.Candidate.Email == model.ApplicantEmail);

            if (alreadyApplied)
            {
                ModelState.AddModelError(string.Empty,
                    "You have already applied for this job.");

                ViewData["Job"] = job;

                return View(model);
            }

            // Resume Upload
            string? fileName = null;

            if (model.ResumeFile != null)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/resumes");

                Directory.CreateDirectory(folder);

                fileName = Guid.NewGuid().ToString()
                           + Path.GetExtension(model.ResumeFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ResumeFile.CopyTo(stream);
                }
            }

            var candidate = new Candidate
            {
                FullName = model.Name,
                Email = model.ApplicantEmail,
                Phone = model.Phone,
                Skills = model.Skills,
                Experience = model.Experience,
                ResumePath = fileName == null
                    ? null
                    : "/resumes/" + fileName
            };

            var application = new Application
            {
                JobId = id,
                Candidate = candidate,
                Status = "Applied",
                AppliedDate = DateTime.UtcNow
            };

            _context.Applications.Add(application);

            _context.SaveChanges();

            TempData["Success"] =
                "Application submitted successfully.";

            return RedirectToAction("Index", "Job");
        }
    }
}
