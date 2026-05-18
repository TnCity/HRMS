using System.ComponentModel.DataAnnotations;

namespace HRMS.Entities
{
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string? ResumePath { get; set; }

        public string? Skills { get; set; }

        public string Experience { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.Now;

        public ICollection<Application>? Applications { get; set; }
    }
}