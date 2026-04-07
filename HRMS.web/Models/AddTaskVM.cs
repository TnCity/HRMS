using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.web.Models
{
    public class AddTaskVM
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public bool IsCompleted { get; set; }

        public List<SelectListItem> Employees { get; set; }
    }
}
