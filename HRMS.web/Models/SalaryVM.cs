using HRMS.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

public class SalaryVM
{
    public SalaryStructure Salary { get; set; } = new SalaryStructure();

    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
}