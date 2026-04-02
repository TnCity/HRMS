using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // ✅ IMPORTANT

namespace HRMS.Entities
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [ValidateNever] 
        public Employee? Employee { get; set; }  

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public string Reason { get; set; }  

        public string Status { get; set; } = "Pending";

        public DateTime AppliedOn { get; set; } = DateTime.Now;
    }
}