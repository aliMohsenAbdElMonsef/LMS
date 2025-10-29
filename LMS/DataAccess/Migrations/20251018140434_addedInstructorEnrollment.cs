using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addedInstructorEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstructorCourses");

            migrationBuilder.CreateTable(
                name: "InstructorEnrollments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorEnrollments_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InstructorEnrollments_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstructorEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorEnrollments_ApplicationUserId",
                table: "InstructorEnrollments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorEnrollments_CourseId",
                table: "InstructorEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorEnrollments_InstructorId_CourseId_Status",
                table: "InstructorEnrollments",
                columns: new[] { "InstructorId", "CourseId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstructorEnrollments");

            migrationBuilder.CreateTable(
                name: "InstructorCourses",
                columns: table => new
                {
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorCourses", x => new { x.InstructorId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_InstructorCourses_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstructorCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorCourses_CourseId",
                table: "InstructorCourses",
                column: "CourseId");
        }
    }
}
