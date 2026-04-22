using HRMS.DAL;
using HRMS.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HRMS.BLL.Services
{
    public class AttendanceService
    {
        private readonly HRMSDbContext _context;

        public AttendanceService(HRMSDbContext context)
        {
            _context = context;
        }

        // 🔹 STEP 1: Insert using Stored Procedure
        public async Task InsertAttendanceAsync(AttendanceLog log)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_InsertAttendanceLog @EmployeeId, @TimeStamp, @PunchType, @DeviceId",
                new SqlParameter("@EmployeeId", log.EmployeeId),
                new SqlParameter("@TimeStamp", log.TimeStamp),
                new SqlParameter("@PunchType", log.PunchType),
                new SqlParameter("@DeviceId", log.DeviceId)
            );

            // 🔥 STEP 2: Immediately update attendance table
            await GenerateAttendanceForDay(log.EmployeeId, log.TimeStamp.Date);
        }

        // 🔹 STEP 2: Generate attendance for that day
        public async Task GenerateAttendanceForDay(int empId, DateTime date)
        {
            var logs = await _context.AttendanceLogs
                .Where(x => x.EmployeeId == empId &&
                x.TimeStamp >= date &&
                x.TimeStamp < date.AddDays(1))
                .ToListAsync();

            if (!logs.Any()) return;

            var hasIn = logs.Any(x => x.PunchType == "IN");
            var hasOut = logs.Any(x => x.PunchType == "OUT");

            string status;

            if (!hasIn)
                status = "Absent";
            else if (!hasOut)
                status = "Incomplete";
            else
                status = "Present";

            var existing = await _context.Attendances
                .FirstOrDefaultAsync(x => x.EmployeeId == empId && x.Date == date);

            if (existing == null)
            {
                _context.Attendances.Add(new Attendance
                {
                    EmployeeId = empId,
                    Date = date,
                    Status = status
                });
            }
            else
            {
                existing.Status = status;
            }

            await _context.SaveChangesAsync();
        }
        public async Task GenerateAbsentForMonth(int empId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var exists = await _context.Attendances
                    .AnyAsync(a => a.EmployeeId == empId && a.Date == date);

                if (!exists)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = empId,
                        Date = date,
                        Status = "Absent"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}