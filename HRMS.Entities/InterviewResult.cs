using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Entities
{
    public class InterviewResult
    {
        [Key]
        public int InterviewResultId { get; set; }

        public int InterviewId { get; set; }

        [ForeignKey("InterviewId")]
        public Interview? Interview { get; set; }

        public string? Feedback { get; set; }

        public string? Result { get; set; }
    }
}