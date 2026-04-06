using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class Performance
    {
        public int Id { get; set; }

        // 🔗 Foreign Key
        public int EmployeeId { get; set; }

        // 🔗 Navigation Property (IMPORTANT)
        public Employee Employee { get; set; }

        public int TasksCompleted { get; set; }
        public int LeavesTaken { get; set; }

        public double ProductivityScore { get; set; } // %
        public int Rating { get; set; } // 1–5
        public string Status { get; set; }

        public DateTime Date { get; set; }
    }
}
