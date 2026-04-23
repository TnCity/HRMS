using System;
using System.Linq;
using System.Threading.Tasks;
using HRMS.DAL;
using HRMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.BLL.Services
{
    public class PerformanceService
    {
        private readonly HRMSDbContext _context;

        public PerformanceService(HRMSDbContext context)
        {
            _context = context;
        }

        public async Task GenerateMonthlyPerformance(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12");

            if (year < 2000)
                throw new ArgumentException("Invalid year");

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            // Get all employees
            var employees = await _context.Employees.ToListAsync();

            // ✅ TASK DATA
            var taskData = await _context.Tasks
                .Where(t => t.IsCompleted &&
                            t.Date >= start &&
                            t.Date < end)
                .GroupBy(t => t.EmployeeId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // ✅ LEAVE DATA (handles date range properly)
            var leaveData = await _context.LeaveRequests
                .Where(l => l.Status == "Approved" &&
                            l.FromDate < end &&
                            l.ToDate >= start)
                .GroupBy(l => l.EmployeeId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            foreach (var emp in employees)
            {
                int tasks = taskData.ContainsKey(emp.EmployeeId)
                    ? taskData[emp.EmployeeId]
                    : 0;

                int leaves = leaveData.ContainsKey(emp.EmployeeId)
                    ? leaveData[emp.EmployeeId]
                    : 0;

                // ✅ SCORE CALCULATION
                int score = (tasks * 5) - (leaves * 2);

                var existing = await _context.MonthlyPerformances
                    .FirstOrDefaultAsync(x =>
                        x.EmployeeId == emp.EmployeeId &&
                        x.Year == year &&
                        x.Month == month);

                if (existing == null)
                {
                    _context.MonthlyPerformances.Add(new MonthlyPerformance
                    {
                        EmployeeId = emp.EmployeeId,
                        Year = year,
                        Month = month,
                        TasksCompleted = tasks,
                        LeavesTaken = leaves,
                        Score = score
                    });
                }
                else
                {
                    // Update existing record
                    existing.TasksCompleted = tasks;
                    existing.LeavesTaken = leaves;
                    existing.Score = score;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}