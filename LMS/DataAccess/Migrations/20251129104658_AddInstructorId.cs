using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class AddInstructorId : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "Lectures",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_InstructorId",
                table: "Lectures",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_AspNetUsers_InstructorId",
                table: "Lectures",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_AspNetUsers_InstructorId",
                table: "Lectures");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_InstructorId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Lectures");
        }
    }
}
