

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.web.Controllers
{
    public class AdminController : Controller
    {
        private readonly HRMSDbContext _context;
        private readonly IConfiguration _config; // ✅ added

        public AdminController(HRMSDbContext context, IConfiguration config) // ✅ updated
        {
            _context = context;
            _config = config;
        }

        //-----------------HttpGet-----------For Admin Register-------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //-----------------HttpPost-----------For Register-------------------
        [HttpPost]
        public IActionResult Register(Admin model)
        {
            if (ModelState.IsValid)
            {
                var exists = _context.Admins.Any(a => a.EmailId == model.EmailId);

                if (exists)
                {
                    ViewBag.Message = "Email already Exist. Try another...";
                    return View();
                }

                _context.Admins.Add(model);
                _context.SaveChanges();

                ViewBag.Message = "Registration successful...";
                return View();
            }

            return View();
        }

        //----------------- Login -------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //----------------- HttpPost Login -------------------
        [HttpPost]
        public IActionResult Login(Admin model)
        {
            var admin = _context.Admins
                .FirstOrDefault(a => a.EmailId == model.EmailId && a.Password == model.Password);

            if (admin == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View();
            }

            HttpContext.Session.SetString("Admin", admin.EmailId);

            // ✅ Token generated (optional for MVC flow)
            var token = GenerateToken(admin);

            return RedirectToAction("Index", "Employee");
        }

        //----------------- API Login (JWT) -------------------
        [HttpPost]
        [Route("api/admin/login")]
        public IActionResult ApiLogin([FromBody] Admin model)
        {
            var admin = _context.Admins
                .FirstOrDefault(a => a.EmailId == model.EmailId && a.Password == model.Password);

            if (admin == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateToken(admin);

            return Ok(new
            {
                token = token
            });
        }

        //------------------------ TOKEN GENERATION -------------------------------
        private string GenerateToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.EmailId),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // ✅ read from appsettings
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],     
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //----------------- Logout -------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}