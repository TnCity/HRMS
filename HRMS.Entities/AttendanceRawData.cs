using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class AttendanceRawData
    {
        public int Id { get; set; } // if exists, else remove

        public int EmployeeCode { get; set; }

        public DateTime Timestamp { get; set; }

        public string DeviceId { get; set; }
    }
}
