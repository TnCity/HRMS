using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResumePath { get; set; }
        public ICollection<Application>? Applications { get; set; }
    }
}
