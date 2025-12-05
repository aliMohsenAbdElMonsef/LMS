using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.DataAccess.Migrations
{

    public partial class FixDaySchedulesTable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("IF OBJECT_ID('DaySchedules', 'U') IS NOT NULL DROP TABLE DaySchedules");


            migrationBuilder.Sql("IF OBJECT_ID('CourseDaySchedule', 'U') IS NOT NULL EXEC sp_rename 'CourseDaySchedule', 'CourseDaySchedules'");


            migrationBuilder.Sql(@"
                IF OBJECT_ID('CourseDaySchedules', 'U') IS NULL
                BEGIN
                    CREATE TABLE [CourseDaySchedules] (
                        [ID] nvarchar(450) NOT NULL,
                        [CourseId] nvarchar(450) NOT NULL,
                        [InstructorId] nvarchar(450) NULL,
                        [DayOfWeek] int NOT NULL,
                        [StartTime] time NOT NULL,
                        [EndTime] time NOT NULL,
                        [IsDeleted] bit NOT NULL,
                        [DeletedAt] datetime2 NULL,
                        CONSTRAINT [PK_CourseDaySchedules] PRIMARY KEY ([ID]),
                        CONSTRAINT [FK_CourseDaySchedules_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_CourseDaySchedules_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id])
                    );
                    CREATE INDEX [IX_CourseDaySchedules_CourseId] ON [CourseDaySchedules] ([CourseId]);
                    CREATE INDEX [IX_CourseDaySchedules_InstructorId] ON [CourseDaySchedules] ([InstructorId]);
                END
            ");


            migrationBuilder.Sql(@"
                IF COL_LENGTH('CourseDaySchedules', 'InstructorId') IS NULL
                BEGIN
                    ALTER TABLE [CourseDaySchedules] ADD [InstructorId] nvarchar(450) NULL;
                    CREATE INDEX [IX_CourseDaySchedules_InstructorId] ON [CourseDaySchedules] ([InstructorId]);
                    ALTER TABLE [CourseDaySchedules] ADD CONSTRAINT [FK_CourseDaySchedules_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]);
                END
            ");
            
            migrationBuilder.Sql(@"
                IF COL_LENGTH('CourseDaySchedules', 'StartTime') IS NULL
                BEGIN
                    ALTER TABLE [CourseDaySchedules] ADD [StartTime] time NOT NULL DEFAULT '00:00:00';
                END
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('CourseDaySchedules', 'EndTime') IS NULL
                BEGIN
                    ALTER TABLE [CourseDaySchedules] ADD [EndTime] time NOT NULL DEFAULT '00:00:00';
                END
            ");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("IF OBJECT_ID('CourseDaySchedules', 'U') IS NOT NULL EXEC sp_rename 'CourseDaySchedules', 'CourseDaySchedule'");
        }
    }
}
