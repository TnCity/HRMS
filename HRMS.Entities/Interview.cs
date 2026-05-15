using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }

        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        public DateTime InterviewDate { get; set; }

        public string InterviewType { get; set; }

        public string InterviewerName { get; set; }

        public string Feedback { get; set; }

        public string Result { get; set; }
    }
}
