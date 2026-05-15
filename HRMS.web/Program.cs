using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;
using System.Globalization;
using System.Text;
using HRMS.BLL.Services;
using HRMS.DAL;
using HRMS.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<HRMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Localization
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// MVC
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<IJobRepository, JobRepository>();

// Services
builder.Services.AddScoped<EmployeeService>();

builder.Services.AddScoped<AttendanceService>();

builder.Services.AddScoped<PerformanceService>();

builder.Services.AddScoped<JobService>();

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();

// JWT
builder.Services.AddAuthentication("JwtAuth")
    .AddJwtBearer("JwtAuth", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            "THIS_IS_MY_SECRET_KEY_12345"))
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Localization cultures
var cultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("bn")
};

app.UseRequestLocalization(
    new RequestLocalizationOptions
    {
        DefaultRequestCulture =
            new RequestCulture("en"),

        SupportedCultures = cultures,

        SupportedUICultures = cultures
    });

// Rotativa
RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa");

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();