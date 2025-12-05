using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{

    public partial class ChangeStudentQuizPrimaryKey : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes");


            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "StudentQuizzes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");


            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes",
                column: "Id");


            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizzes_StudentId_QuizId",
                table: "StudentQuizzes",
                columns: new[] { "StudentId", "QuizId" });
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizzes_StudentId_QuizId",
                table: "StudentQuizzes");


            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes");


            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentQuizzes");


            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes",
                columns: new[] { "StudentId", "QuizId" });
        }
    }
}
