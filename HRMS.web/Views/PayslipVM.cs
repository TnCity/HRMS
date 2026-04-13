using HRMS.Entities;

namespace HRMS.web.Views
{
    public class PayslipVM
    {
        public Payroll Payroll { get; set; }
        public SalaryStructure? Salary { get; set; }
    }
}
