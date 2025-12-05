using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class assignmentstatusadd : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "StudentAssignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StudentAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "StudentAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentAssignments");
        }
    }
}
