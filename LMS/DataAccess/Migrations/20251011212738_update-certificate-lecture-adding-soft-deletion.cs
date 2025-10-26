using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updatecertificatelectureaddingsoftdeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseReviews_AspNetUsers_StudentId",
                table: "CourseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseReviews_Courses_CourseId",
                table: "CourseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSkills_Courses_CourseId",
                table: "CourseSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSkills_Skills_SkillId",
                table: "CourseSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_AspNetUsers_InstructorId",
                table: "InstructorCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswers_AspNetUsers_StudentId",
                table: "StudentAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswers_Questions_QuestionId",
                table: "StudentAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAssignments_AspNetUsers_StudentId",
                table: "StudentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAssignments_Assignments_AssignmentId",
                table: "StudentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCertificates_AspNetUsers_StudentId",
                table: "StudentCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCertificates_CertificateTemplates_certificateTamplateId",
                table: "StudentCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_AspNetUsers_StudentId",
                table: "StudentEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentLectures_AspNetUsers_StudentId",
                table: "StudentLectures");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentLectures_Lectures_LectureId",
                table: "StudentLectures");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_AspNetUsers_StudentId",
                table: "StudentQuizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizId",
                table: "StudentQuizzes");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "EndHour",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "TemplateContent",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "TemplatePath",
                table: "CertificateTemplates");

            migrationBuilder.RenameColumn(
                name: "StartHour",
                table: "Lectures",
                newName: "StartTime");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Lectures",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "LectureDate",
                table: "Lectures",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LectureScheduleId",
                table: "Lectures",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateTemplateID",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "AutoIssueCertificates",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MinAttendancePercentage",
                table: "Courses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinPerformanceScore",
                table: "Courses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CertificateTemplates",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageBody",
                table: "CertificateTemplates",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CertificateTemplates",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LectureSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LectureSchedules_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_LectureScheduleId",
                table: "Lectures",
                column: "LectureScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_LectureSchedules_CourseId",
                table: "LectureSchedules",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_AspNetUsers_StudentId",
                table: "CourseReviews",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_Courses_CourseId",
                table: "CourseReviews",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSkills_Courses_CourseId",
                table: "CourseSkills",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSkills_Skills_SkillId",
                table: "CourseSkills",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_AspNetUsers_InstructorId",
                table: "InstructorCourses",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_LectureSchedules_LectureScheduleId",
                table: "Lectures",
                column: "LectureScheduleId",
                principalTable: "LectureSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswers_AspNetUsers_StudentId",
                table: "StudentAnswers",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswers_Questions_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAssignments_AspNetUsers_StudentId",
                table: "StudentAssignments",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAssignments_Assignments_AssignmentId",
                table: "StudentAssignments",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCertificates_AspNetUsers_StudentId",
                table: "StudentCertificates",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCertificates_CertificateTemplates_certificateTamplateId",
                table: "StudentCertificates",
                column: "certificateTamplateId",
                principalTable: "CertificateTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_AspNetUsers_StudentId",
                table: "StudentEnrollments",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLectures_AspNetUsers_StudentId",
                table: "StudentLectures",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLectures_Lectures_LectureId",
                table: "StudentLectures",
                column: "LectureId",
                principalTable: "Lectures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_AspNetUsers_StudentId",
                table: "StudentQuizzes",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizId",
                table: "StudentQuizzes",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseReviews_AspNetUsers_StudentId",
                table: "CourseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseReviews_Courses_CourseId",
                table: "CourseReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSkills_Courses_CourseId",
                table: "CourseSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSkills_Skills_SkillId",
                table: "CourseSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_AspNetUsers_InstructorId",
                table: "InstructorCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_LectureSchedules_LectureScheduleId",
                table: "Lectures");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswers_AspNetUsers_StudentId",
                table: "StudentAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswers_Questions_QuestionId",
                table: "StudentAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAssignments_AspNetUsers_StudentId",
                table: "StudentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAssignments_Assignments_AssignmentId",
                table: "StudentAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCertificates_AspNetUsers_StudentId",
                table: "StudentCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCertificates_CertificateTemplates_certificateTamplateId",
                table: "StudentCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_AspNetUsers_StudentId",
                table: "StudentEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentLectures_AspNetUsers_StudentId",
                table: "StudentLectures");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentLectures_Lectures_LectureId",
                table: "StudentLectures");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_AspNetUsers_StudentId",
                table: "StudentQuizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizId",
                table: "StudentQuizzes");

            migrationBuilder.DropTable(
                name: "LectureSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_LectureScheduleId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "LectureDate",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "LectureScheduleId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "AutoIssueCertificates",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "MinAttendancePercentage",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "MinPerformanceScore",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "MessageBody",
                table: "CertificateTemplates");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CertificateTemplates");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Lectures",
                newName: "StartHour");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "Lectures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndHour",
                table: "Lectures",
                type: "time",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificateTemplateID",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateContent",
                table: "CertificateTemplates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplatePath",
                table: "CertificateTemplates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_AspNetUsers_StudentId",
                table: "CourseReviews",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseReviews_Courses_CourseId",
                table: "CourseReviews",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSkills_Courses_CourseId",
                table: "CourseSkills",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSkills_Skills_SkillId",
                table: "CourseSkills",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_AspNetUsers_InstructorId",
                table: "InstructorCourses",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InstructorCourses_Courses_CourseId",
                table: "InstructorCourses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Quizzes_QuizId",
                table: "Questions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswers_AspNetUsers_StudentId",
                table: "StudentAnswers",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswers_Questions_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAssignments_AspNetUsers_StudentId",
                table: "StudentAssignments",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAssignments_Assignments_AssignmentId",
                table: "StudentAssignments",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCertificates_AspNetUsers_StudentId",
                table: "StudentCertificates",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCertificates_CertificateTemplates_certificateTamplateId",
                table: "StudentCertificates",
                column: "certificateTamplateId",
                principalTable: "CertificateTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_AspNetUsers_StudentId",
                table: "StudentEnrollments",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLectures_AspNetUsers_StudentId",
                table: "StudentLectures",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentLectures_Lectures_LectureId",
                table: "StudentLectures",
                column: "LectureId",
                principalTable: "Lectures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_AspNetUsers_StudentId",
                table: "StudentQuizzes",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizId",
                table: "StudentQuizzes",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
