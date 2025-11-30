using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class QuizError3 : Migration
    {
        /// <inheritdoc />
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
                name: "IX_StudentQuizzes_StudentId",
                table: "StudentQuizzes",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes");

            migrationBuilder.DropIndex(
                name: "IX_StudentQuizzes_StudentId",
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
