namespace HRMS.web.Models
{
    public class PerformanceVM
    {
        public int EmployeeId { get; set; }  

        public string EmployeeName { get; set; }

        public int TasksCompleted { get; set; }

        public int LeavesTaken { get; set; }

        public int ProductivityScore { get; set; }

        public int Rating { get; set; }

        public string Status { get; set; }
    }
}

