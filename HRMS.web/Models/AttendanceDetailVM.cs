namespace HRMS.web.Models
{
    public class AttendanceDetailVM
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public DateTime? Login { get; set; }
        public DateTime? Logout { get; set; }
        public decimal TotalHours { get; set; }
        public decimal WorkingHours { get; set; }
        public List<string> Punches { get; set; }
    }
}
