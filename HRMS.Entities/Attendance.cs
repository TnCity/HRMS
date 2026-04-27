using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    
    public class Attendance
    {
        public int Id { get; set; }

        // Attendance date (per day)
        public DateTime Date { get; set; }

        // Present / Absent / HalfDay / Leave
        public string Status { get; set; } = "Absent";

        // Foreign key
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        // Punch timings
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }

        // Time calculations
        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalHours { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal BreakHours { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal WorkingHours { get; set; } = 0;
    }

}
