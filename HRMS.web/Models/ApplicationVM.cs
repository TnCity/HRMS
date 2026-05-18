using System.ComponentModel.DataAnnotations;

namespace HRMS.web.Models
{
    public class ApplicationVM
    {
        [Required]
        public string? Name { get; set; }

        [Required, EmailAddress]
        public string? ApplicantEmail { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        public string? Experience { get; set; }

        [Required]
        public string? Skills { get; set; }

        [Required]
        public IFormFile? ResumeFile { get; set; }

        

        public string? CoverLetter { get; set; }

    }
}
