using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class AddInstructorToLectureSchedule : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "LectureSchedules",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LectureSchedules_InstructorId",
                table: "LectureSchedules",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LectureSchedules_AspNetUsers_InstructorId",
                table: "LectureSchedules",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LectureSchedules_AspNetUsers_InstructorId",
                table: "LectureSchedules");

            migrationBuilder.DropIndex(
                name: "IX_LectureSchedules_InstructorId",
                table: "LectureSchedules");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "LectureSchedules");
        }
    }
}
