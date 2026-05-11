using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.DAL.Repositories;
using HRMS.Entities;
using Microsoft.AspNetCore.Identity;

namespace HRMS.BLL.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly PasswordHasher<Employee> _passwordHasher;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
            _passwordHasher = new PasswordHasher<Employee>();
        }

        public List<Employee> GetEmployees()
        {
            return _repo.GetAll().ToList();
        }

        public int GetEmployeeCount()
        {
            return _repo.GetAll().Count();
        }

        public void AddEmployee(Employee emp)
        {
            // Default password
            string defaultPassword = "123";

            // First login
            emp.IsFirstLogin = true;

            // Save hashed password
            emp.Password =
                _passwordHasher.HashPassword(emp, defaultPassword);

            _repo.Add(emp);
        }

        public Employee GetEmployeeById(int id)
        {
            return _repo.GetById(id);
        }

        public IEnumerable<Department> GetDepartments()
        {
            return _repo.GetDepartments();
        }

        public void UpdateEmployee(Employee emp)
        {
            var existingEmp = _repo.GetById(emp.EmployeeId);

            if (existingEmp == null)
                return;

            // Keep old password if empty
            if (string.IsNullOrWhiteSpace(emp.Password))
            {
                emp.Password = existingEmp.Password;
            }
            else
            {
                // Hash new password
                emp.Password =
                    _passwordHasher.HashPassword(emp, emp.Password);
            }

            _repo.Update(emp);
        }

        public void DeleteEmployee(int id)
        {
            _repo.Delete(id);
        }

        public Employee Login(string email, string password)
        {
            email = email.Trim();
            password = password.Trim();

            var employee = _repo.GetByEmail(email);

            if (employee == null)
                return null;

            // =========================
            // OLD PLAIN TEXT PASSWORD
            // =========================
            if (!employee.Password.StartsWith("AQAAAA"))
            {
                if (employee.Password.Trim() == password)
                {
                    // Convert old password to hashed password
                    employee.Password =
                        _passwordHasher.HashPassword(employee, password);

                    _repo.Update(employee);

                    return employee;
                }

                return null;
            }

            // =========================
            // HASHED PASSWORD
            // =========================
            var result = _passwordHasher.VerifyHashedPassword(
                employee,
                employee.Password,
                password
            );

            if (result == PasswordVerificationResult.Success)
            {
                return employee;
            }

            return null;
        }

        public Employee GetByEmail(string email)
        {
            return _repo.GetByEmail(email);
        }
    }
}