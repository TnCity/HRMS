

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.DAL
{
    public class HRMSDbContext : DbContext
    {
        public HRMSDbContext(DbContextOptions<HRMSDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Performance> Performances { get; set; }
        public DbSet<MonthlyPerformance> MonthlyPerformances { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<AttendanceRawData> AttendanceRawDatas { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ FIX: Salary precision issue
            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);


            //modelBuilder.Entity<AttendanceRawData>()
            //.ToTable("Attendence_Raw_Data");

            modelBuilder.Entity<AttendanceRawData>()
            .ToTable("Attendence_Raw_Data", t => t.ExcludeFromMigrations());

            // SalaryStructure relation
            modelBuilder.Entity<SalaryStructure>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payroll relation
            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}