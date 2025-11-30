using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStudentQuizPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing composite primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes");

            // Add the new Id column
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "StudentQuizzes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Set the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes",
                column: "Id");

            // Create an index on StudentId and QuizId for performance
            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizzes_StudentId_QuizId",
                table: "StudentQuizzes",
                columns: new[] { "StudentId", "QuizId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index
            migrationBuilder.DropIndex(
                name: "IX_StudentQuizzes_StudentId_QuizId",
                table: "StudentQuizzes");

            // Drop the new primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes");

            // Drop the Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentQuizzes");

            // Restore the composite primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentQuizzes",
                table: "StudentQuizzes",
                columns: new[] { "StudentId", "QuizId" });
        }
    }
}
