using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        // ✅ Restore relation
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
