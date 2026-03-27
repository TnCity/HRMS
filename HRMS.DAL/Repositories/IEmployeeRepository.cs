using HRMS.Entities;

namespace HRMS.DAL.Repositories
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAll();
        Employee GetById(int id);
        void Add(Employee employee);
        void Update(Employee employee);
        void Delete(int id);
        IEnumerable<Department> GetDepartments();
    }
}