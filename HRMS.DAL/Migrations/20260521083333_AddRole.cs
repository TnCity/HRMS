using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);

            // SET DEFAULT ROLE FOR OLD RECORDS
            migrationBuilder.Sql(
                "UPDATE Admins SET Role = 'Admin' WHERE Role IS NULL"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Admins");
        }
    }
}