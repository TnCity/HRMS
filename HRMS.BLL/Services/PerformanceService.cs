using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.DAL;
using HRMS.Entities;

namespace HRMS.BLL.Services
{
    public class PerformanceService
    {
        private readonly HRMSDbContext _context;

        public PerformanceService(HRMSDbContext context)
        {
            _context = context;
        }

        public void GenerateMonthlyPerformance(int year, int month)
        {
            var employees = _context.Employees.ToList();
            

            var taskData = _context.Tasks
                .Where(t => t.Date.Month == month && t.Date.Year == year && t.IsCompleted)
                .GroupBy(t => t.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var leaveData = _context.LeaveRequests
                .Where(l => l.Status == "Approved" &&
                            l.FromDate.Month == month &&
                            l.FromDate.Year == year)
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var emp in employees)
            {
                int tasks = taskData.ContainsKey(emp.EmployeeId) ? taskData[emp.EmployeeId] : 0;
                int leaves = leaveData.ContainsKey(emp.EmployeeId) ? leaveData[emp.EmployeeId] : 0;

                int score = (tasks * 5) - (leaves * 2);

                var existing = _context.MonthlyPerformances
                    .FirstOrDefault(x => x.EmployeeId == emp.EmployeeId &&
                                         x.Month == month &&
                                         x.Year == year);

                if (existing == null)
                {
                    _context.MonthlyPerformances.Add(new MonthlyPerformance
                    {
                        EmployeeId = emp.EmployeeId,
                        Month = month,
                        Year = year,
                        TasksCompleted = tasks,
                        LeavesTaken = leaves,
                        Score = score
                    });
                }
                else
                {
                    existing.TasksCompleted = tasks;
                    existing.LeavesTaken = leaves;
                    existing.Score = score;
                }
            }

            _context.SaveChanges();
        }
    }
}
