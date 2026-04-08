using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Entities
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // Navigation
        public Employee Employee { get; set; }
    }
}