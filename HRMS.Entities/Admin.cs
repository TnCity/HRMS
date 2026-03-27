using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Admin
    {
        public int Id { get; set; }


        [Required(ErrorMessage ="Email Id is requied")]
        public string EmailId { get; set; }


        [Required(ErrorMessage ="Insert Password")]
        public string Password { get; set; }
    }
}
