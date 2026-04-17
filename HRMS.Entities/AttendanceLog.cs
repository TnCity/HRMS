using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class AttendanceLog
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime TimeStamp { get; set; }
        public string PunchType { get; set; }
        public string DeviceId { get; set; }

    }
}
