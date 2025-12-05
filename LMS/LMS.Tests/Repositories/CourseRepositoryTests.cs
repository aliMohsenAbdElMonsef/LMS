using DataAccess.Context;
using LMS.DataAccess.Repositories;
using Domain.Entities.MainEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LMS.Tests.Repositories
{
    public class CourseRepositoryTests
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public CourseRepositoryTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }
        private LMSDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LMSDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new LMSDbContext(options);
        }



        [Fact]
        public async Task FindByIdAsync_ShouldIncludeQuizzes()
        {

            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<LMSDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            using (var context = new LMSDbContext(options))
            {
                var admin = new Domain.Entities.MainEntities.ApplicationUser { Id = "admin1", UserName = "admin" };
                var category = new Domain.Entities.MainEntities.Category { Id = "cat1", Name = "Test Cat", AdminId = "admin1" };

                var course = new Course
                {
                    Id = "course1",
                    Name = "Test Course",
                    AdminId = "admin1",
                    CategoryId = "cat1",
                    Language = "English",
                    EveryStuCouldEnroll = true
                };

                var quiz = new Quiz
                {
                    Id = "quiz1",
                    Title = "Test Quiz",
                    CourseId = "course1",
                    InstructorId = "inst1"
                };

                context.Users.Add(admin);
                context.Categories.Add(category);
                context.Courses.Add(course);
                context.Quizzes.Add(quiz);
                await context.SaveChangesAsync();
            }


            using (var context = new LMSDbContext(options))
            {
                var repository = new CourseRepository(context);
                var result = await repository.FindByIdAsync("course1");


                Assert.NotNull(result);
                Assert.NotNull(result.Quizzes);
                Assert.Single(result.Quizzes);
                Assert.Equal("quiz1", result.Quizzes.First().Id);
            }
        }

    }
}
