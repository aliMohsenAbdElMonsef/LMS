using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class addingchoiceonstudentenrollment : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EveryStuCouldEnroll",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EveryStuCouldEnroll",
                table: "Courses");
        }
    }
}
