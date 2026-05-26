using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInterviewRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewResults_InterviewId",
                table: "InterviewResults");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewResults_InterviewId",
                table: "InterviewResults",
                column: "InterviewId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewResults_InterviewId",
                table: "InterviewResults");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewResults_InterviewId",
                table: "InterviewResults",
                column: "InterviewId");
        }
    }
}
