

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HRMS.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress] 
        public string Email { get; set; }
        
        public string? Password { get; set; }
        public bool IsFirstLogin { get; set; } = true;

        [Required]
        [Phone]  
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select a department")]
        public int? DepartmentId { get; set; }

        [ValidateNever]
        public Department? Department { get; set; }

        public string? Role { get; set; } 

        [Required]
        public DateTime JoinDate { get; set; }

        public string? ProfileImagePath { get; set; }

        [Required] 
        public string Address { get; set; }

        [Required]
        [Range(1000, 1000000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        [Required]
        public string Designation { get; set; }
        public int? EmployeeCode { get; set; }
    }
}