using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Migrations
{
    /// <inheritdoc />
    public partial class removeskills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseSkills");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Skills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LecturesHours",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CourseId",
                table: "Skills",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_courses_CourseId",
                table: "Skills",
                column: "CourseId",
                principalTable: "courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_courses_CourseId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_CourseId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "LecturesHours",
                table: "Skills");

            migrationBuilder.CreateTable(
                name: "CourseSkills",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    LecturesHours = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSkills", x => new { x.CourseId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_CourseSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseSkills_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSkills_SkillId",
                table: "CourseSkills",
                column: "SkillId");
        }
    }
}
