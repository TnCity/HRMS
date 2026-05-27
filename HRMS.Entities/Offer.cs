using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }

        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application? Application { get; set; }

        public DateTime OfferDate { get; set; }

        public DateTime JoiningDate { get; set; }

        public decimal Salary { get; set; }

        public string? OfferStatus { get; set; }
    }
}
