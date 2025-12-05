using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class initaftermerge : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "StudentEnrollments");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentQuizzes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentQuizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentLectures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentLectures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentEnrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentEnrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentCertificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentCertificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InstructorEnrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InstructorEnrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CourseSkills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CourseSkills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CourseReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CourseReviews",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentQuizzes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentQuizzes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentLectures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentLectures");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentCertificates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentCertificates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentAssignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InstructorEnrollments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InstructorEnrollments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CourseSkills");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CourseSkills");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CourseReviews");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CourseReviews");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "StudentEnrollments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
