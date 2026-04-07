using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Entities
{
    public class MonthlyPerformance
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int TasksCompleted { get; set; }
        public int LeavesTaken { get; set; }

        public int Score { get; set; }
    }
}
