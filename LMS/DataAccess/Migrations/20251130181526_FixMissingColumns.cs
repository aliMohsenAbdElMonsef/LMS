using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class FixMissingColumns : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'InstructorId' AND Object_ID = Object_ID(N'Quizzes'))
                BEGIN
                    ALTER TABLE [Quizzes] ADD [InstructorId] nvarchar(450) NOT NULL DEFAULT '';
                    CREATE INDEX [IX_Quizzes_InstructorId] ON [Quizzes] ([InstructorId]);
                    ALTER TABLE [Quizzes] ADD CONSTRAINT [FK_Quizzes_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'InstructorId' AND Object_ID = Object_ID(N'LectureSchedules'))
                BEGIN
                    ALTER TABLE [LectureSchedules] ADD [InstructorId] nvarchar(450) NOT NULL DEFAULT '';
                    CREATE INDEX [IX_LectureSchedules_InstructorId] ON [LectureSchedules] ([InstructorId]);
                    ALTER TABLE [LectureSchedules] ADD CONSTRAINT [FK_LectureSchedules_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'StartTime' AND Object_ID = Object_ID(N'LectureSchedules'))
                BEGIN
                    ALTER TABLE [LectureSchedules] ADD [StartTime] time NOT NULL DEFAULT '00:00:00';
                END
            ");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
