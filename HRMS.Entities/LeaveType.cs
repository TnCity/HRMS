using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public enum LeaveType
    {
        [Display(Name = "Casual Leave")]
        CL,

        [Display(Name = "Sick Leave")]
        SL,

        [Display(Name = "Paid Leave")]
        PL,

        [Display(Name = "Loss of Pay")]
        LOP
    }
}
