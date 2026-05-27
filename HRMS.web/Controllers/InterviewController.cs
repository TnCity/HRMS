using System.Configuration;
using System.Net;
using System.Net.Mail;
using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.web.Controllers
{
    public class InterviewController : Controller
    {
        private readonly HRMSDbContext _context;
        private readonly IConfiguration _configuration;

        public InterviewController(HRMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // ================= SEND EMAIL =================

            var application = _context.Applications
                .Include(a => a.Candidate)
                .FirstOrDefault(a =>
                    a.ApplicationId == model.ApplicationId);

            if (application != null)
            {
                try
                {
                    var email =
                        _configuration["EmailSettings:Email"];

                    var password =
                        _configuration["EmailSettings:Password"];

                    var host =
                        _configuration["EmailSettings:Host"];

                    var port =
                        int.Parse(
                            _configuration["EmailSettings:Port"]);

                    MailMessage mail = new MailMessage();

                    mail.From =
                        new MailAddress(email);

                    mail.To.Add(application.Candidate.Email);

                    mail.Subject =
                        "Interview Schedule At ABC Pvt Ltd Company";

                    mail.Body =
                    $@"Dear {application.Candidate.FullName},

                    Your interview has been scheduled successfully.

                    Interview Details:

                    Date:
                    {model.InterviewDate:dd MMM yyyy hh:mm tt}

                    Type:
                    {model.InterviewType}

                    Please be available on time.

                    Regards,
                    HR Team";

                    mail.IsBodyHtml = false;

                    SmtpClient smtp =
                        new SmtpClient(host, port);

                    smtp.Credentials =
                        new NetworkCredential(
                            email,
                            password);

                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            TempData["Success"] =
                "Interview scheduled and email sent successfully.";

            return RedirectToAction("AppliedJob","Job");
        }

        // ================================== INTERVIEW RESULT ===================================================

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
                    application.FinalResult = model.Result;
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

            var interviews = _context.Interviews
                .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
                .Include(i => i.Application.Job)
                .ToList();

            return View(interviews);
        }

        //--------------------------Generate offer Letter-----------------------------------------


        [HttpGet]
        public IActionResult GenerateOffer(int applicationId)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var application = _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .FirstOrDefault(a => a.ApplicationId == applicationId);

            if (application == null)
            {
                return NotFound();
            }

            Offer offer = new Offer()
            {
                ApplicationId = application.ApplicationId,
                OfferDate = DateTime.Now,
                JoiningDate = DateTime.Now.AddDays(15),
                Salary = 0,
                OfferStatus = "Pending"
            };

            ViewBag.Candidate =
                application.Candidate?.FullName;

            ViewBag.Job =
                application.Job?.Title;

            return View(offer);
        }

        [HttpPost]
        public IActionResult GenerateOffer(Offer model)
        {
            if (!ModelState.IsValid)
            {
                var application = _context.Applications
                    .Include(a => a.Candidate)
                    .Include(a => a.Job)
                    .FirstOrDefault(a => a.ApplicationId == model.ApplicationId);

                ViewBag.Candidate =
                    application?.Candidate?.FullName;

                ViewBag.Job =
                    application?.Job?.Title;

                return View(model);
            }

            _context.Offers.Add(model);

            _context.SaveChanges();

            TempData["Success"] =
                "Offer letter generated successfully.";

            return RedirectToAction(
                "AppliedJob",
                "Job");
        }
    }
}