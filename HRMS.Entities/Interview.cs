using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Entities
{
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }

        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application? Application { get; set; }

        public DateTime InterviewDate { get; set; }

        public string? InterviewType { get; set; }
    }
}