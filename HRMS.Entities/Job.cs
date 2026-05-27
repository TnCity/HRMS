using System.ComponentModel.DataAnnotations;

namespace HRMS.Entities
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Department { get; set; }

        [Required]
        public string? Description { get; set; }

        public string? ExperienceRequired { get; set; }

        public string? SalaryRange { get; set; }

        public string? Location { get; set; }

        public DateTime LastDate { get; set; }

        public string? Status { get; set; } = "Open";

        public ICollection<Application>? Applications { get; set; }
    }
}