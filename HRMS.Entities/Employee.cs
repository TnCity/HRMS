




using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HRMS.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress] // ✅ important
        public string Email { get; set; }

        [Required]
        [Phone] // ✅ better validation
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select a department")]
        public int? DepartmentId { get; set; }

        [ValidateNever]
        public Department? Department { get; set; }

        public string? Role { get; set; } // ✅ nullable (optional)

        [Required]
        public DateTime JoinDate { get; set; }

        public string? ProfileImagePath { get; set; }

        [Required] // ✅ important
        public string Address { get; set; }

        [Required]
        [Range(1000, 1000000)] // ✅ validation
        public decimal Salary { get; set; }

        [Required]
        public string Designation { get; set; }
    }
}