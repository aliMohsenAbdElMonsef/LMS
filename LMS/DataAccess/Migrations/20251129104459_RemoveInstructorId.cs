using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class RemoveInstructorId : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {











        }


        protected override void Down(MigrationBuilder migrationBuilder)
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
                onDelete: ReferentialAction.Cascade);
        }
    }
}
