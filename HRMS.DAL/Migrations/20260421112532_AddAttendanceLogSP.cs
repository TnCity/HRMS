using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    public partial class AddAttendanceLogSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_InsertAttendanceLog
                    @EmployeeId INT,
                    @TimeStamp DATETIME,
                    @PunchType NVARCHAR(10),
                    @DeviceId NVARCHAR(50)
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Prevent weekend (Sunday=1, Saturday=7)
                    SET DATEFIRST 7;

                    IF DATEPART(WEEKDAY, @TimeStamp) IN (1,7)
                        RETURN;

                    -- Prevent duplicate
                    IF EXISTS (
                        SELECT 1 
                        FROM AttendanceLogs
                        WHERE EmployeeId = @EmployeeId
                        AND TimeStamp = @TimeStamp
                        AND PunchType = @PunchType
                    )
                        RETURN;

                    -- Insert record
                    INSERT INTO AttendanceLogs (EmployeeId, TimeStamp, PunchType, DeviceId)
                    VALUES (@EmployeeId, @TimeStamp, @PunchType, @DeviceId);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_InsertAttendanceLog");
        }
    }
}