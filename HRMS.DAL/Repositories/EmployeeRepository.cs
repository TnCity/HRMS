using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HRMSDbContext _context;

        public EmployeeRepository(HRMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Employee> GetAll()
        {
            return _context.Employees.Include(e => e.Department).ToList();
        }

        public Employee GetById(int id)
        {
            return _context.Employees
                           .Include(e => e.Department)
                           .FirstOrDefault(e => e.EmployeeId == id);
        }

        public void Add(Employee employee)
        {
            


            _context.Employees.Add(employee);
            _context.SaveChanges();
        }
        public IEnumerable<Department> GetDepartments()
        {
            return _context.Departments.ToList();
        }
        public void Update(Employee employee)
        {
            var existing = _context.Employees
                .FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);

            if (existing != null)
            {
                existing.Name = employee.Name;
                existing.Email = employee.Email;
                existing.DepartmentId = employee.DepartmentId;
                existing.Salary = employee.Salary;

                // Preserve password
                if (!string.IsNullOrEmpty(employee.Password))
                {
                    existing.Password = employee.Password;
                }
            }

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var emp = _context.Employees.Find(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                _context.SaveChanges();
            }
        }
        public Employee GetByEmailAndPassword(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var trimmedPassword = password.Trim();

            // Retrieve a tracked entity so callers can update flags (e.g., IsFirstLogin)
            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail);

            if (employee == null) return null;

            // Simple plain-text comparison for now (replace with hashing in production)
            if (string.Equals(employee.Password?.Trim(), trimmedPassword, StringComparison.Ordinal))
                return employee;

            return null;
        }


        public Employee GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return _context.Employees
                .FirstOrDefault(x => x.Email != null && x.Email.ToLower() == normalizedEmail);
        }
    }
}