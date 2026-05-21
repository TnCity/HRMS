




using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HRMS.DAL;
using HRMS.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.web.Controllers
{
    public class AdminController : Controller
    {
        private readonly HRMSDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Admin> _passwordHasher;

        public AdminController(HRMSDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _passwordHasher = new PasswordHasher<Admin>();
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Admin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = _context.Admins
                .Any(a => a.EmailId == model.EmailId);

            if (exists)
            {
                ViewBag.Message = "Email already exists!";
                return View(model);
            }

            // HASH PASSWORD
            model.Password =
                _passwordHasher.HashPassword(model, model.Password);

            _context.Admins.Add(model);

            _context.SaveChanges();

            TempData["Success"] = "Registration successful!";

            return RedirectToAction("Login");
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //public IActionResult Login(Admin model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var admin = _context.Admins
        //        .FirstOrDefault(a => a.EmailId == model.EmailId);

        //    if (admin == null)
        //    {
        //        ViewBag.Error = "Invalid Email or Password";
        //        return View(model);
        //    }

        //    // VERIFY HASHED PASSWORD
        //    var result = _passwordHasher.VerifyHashedPassword(
        //        admin,
        //        admin.Password,
        //        model.Password
        //    );

        //    if (result == PasswordVerificationResult.Success)
        //    {
        //        HttpContext.Session.SetString("Admin", admin.EmailId);

        //        return RedirectToAction("Dashboard");
        //    }

        //    ViewBag.Error = "Invalid Email or Password";

        //    return View(model);
        //}




        //--------------------------test--------------------------

        public IActionResult Login(Admin model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var admin = _context.Admins
                .FirstOrDefault(a => a.EmailId == model.EmailId);

            if (admin == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View(model);
            }

            bool isValidPassword = false;

            // ✅ Check if password is hashed
            if (admin.Password.StartsWith("AQAAAA"))
            {
                // Verify hashed password
                var result = _passwordHasher.VerifyHashedPassword(
                    admin,
                    admin.Password,
                    model.Password
                );

                isValidPassword = result == PasswordVerificationResult.Success;
            }
            else
            {
                // Old plain text password support
                isValidPassword = admin.Password == model.Password;

                // Optional: auto convert old password to hashed
                if (isValidPassword)
                {
                    admin.Password = _passwordHasher.HashPassword(admin, model.Password);
                    _context.SaveChanges();
                }
            }

            if (isValidPassword)
            {
                HttpContext.Session.SetString("Admin", admin.EmailId);

                HttpContext.Session.SetString("Role", admin.Role);


                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid Email or Password";

            return View(model);
        }

        // ================= DASHBOARD =================

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Admin") == null)
                return RedirectToAction("Login");

            ViewBag.AdminEmail =
                HttpContext.Session.GetString("Admin");

            return View();
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        // ================= API LOGIN (JWT) =================

        [HttpPost]
        [Route("api/admin/login")]
        public IActionResult ApiLogin([FromBody] Admin model)
        {
            var admin = _context.Admins
                .FirstOrDefault(a => a.EmailId == model.EmailId);

            if (admin == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                admin,
                admin.Password,
                model.Password
            );

            if (result != PasswordVerificationResult.Success)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateToken(admin);

            return Ok(new
            {
                token = token
            });
        }

        // ================= TOKEN GENERATION =================

        private string GenerateToken(Admin admin)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.EmailId),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}