using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class AddTitleAndDescToLectureSchedule : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LectureSchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LectureSchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "LectureSchedules");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "LectureSchedules");
        }
    }
}
