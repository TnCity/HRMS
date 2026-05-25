using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Entities
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public Job? Job { get; set; }

        public int CandidateId { get; set; }

        [ForeignKey("CandidateId")]
        public Candidate? Candidate { get; set; }

        public string Status { get; set; } = "Applied";

        public DateTime AppliedDate { get; set; } = DateTime.Now;

       
    }
}