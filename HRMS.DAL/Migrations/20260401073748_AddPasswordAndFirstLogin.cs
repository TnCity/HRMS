using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordAndFirstLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "Password",
            table: "Employees",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "123"
);

            migrationBuilder.AddColumn<bool>(
            name: "IsFirstLogin",
            table: "Employees",
            type: "bit",
            nullable: false,
            defaultValue: true
);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Employees");
        }
    }
}
