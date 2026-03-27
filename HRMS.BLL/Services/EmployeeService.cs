using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.DAL.Repositories;
using HRMS.Entities;

namespace HRMS.BLL.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
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
            _repo.Update(emp);
        }

        public void DeleteEmployee(int id)
        {
            _repo.Delete(id);
        }
    }
}
