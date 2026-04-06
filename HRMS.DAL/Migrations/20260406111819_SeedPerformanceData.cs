using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    public partial class SeedPerformanceData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Performances",
                columns: new[]
                {
                    "Id", "EmployeeId", "TasksCompleted", "LeavesTaken",
                    "ProductivityScore", "Rating", "Status", "Date"
                },
                values: new object[,]
                {
                    { 1, 14, 20, 1, 100, 5, "Good", new DateTime(2026, 4, 6) },
                    { 2, 15, 18, 1, 90, 5, "Good", new DateTime(2026, 4, 6) },
                    { 3, 16, 15, 2, 75, 4, "Good", new DateTime(2026, 4, 6) },
                    { 4, 17, 12, 2, 60, 4, "Good", new DateTime(2026, 4, 6) },
                    { 5, 18, 10, 3, 50, 3, "Average", new DateTime(2026, 4, 6) },
                    { 6, 20, 8, 3, 40, 3, "Average", new DateTime(2026, 4, 6) },
                    { 7, 21, 6, 4, 30, 2, "Poor", new DateTime(2026, 4, 6) },
                    { 8, 23, 4, 5, 20, 1, "Poor", new DateTime(2026, 4, 6) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Performances",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    1,2,3,4,5,6,7,8
                });
        }
    }
}