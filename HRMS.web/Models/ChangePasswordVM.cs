using System.ComponentModel.DataAnnotations;

namespace HRMS.web.Models
{
    public class ChangePasswordVM
    {
        public int EmployeeId { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
