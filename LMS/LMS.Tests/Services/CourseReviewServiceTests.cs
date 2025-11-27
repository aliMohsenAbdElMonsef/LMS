using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CourseReview;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace LMS.Tests.Services
{
    public class CourseReviewServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CourseReviewService _service;

        public CourseReviewServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new CourseReviewService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetCourseRatingStatsAsync_CalculatesCorrectAverage()
        {
            // Arrange
            var courseId = "course1";
            var reviews = new List<CourseReview>
            {
                new CourseReview { Id = "1", CourseId = courseId, Rating = 5, IsDeleted = false, Course = new Course { Name = "Test Course" } },
                new CourseReview { Id = "2", CourseId = courseId, Rating = 4, IsDeleted = false, Course = new Course { Name = "Test Course" } },
                new CourseReview { Id = "3", CourseId = courseId, Rating = 3, IsDeleted = false, Course = new Course { Name = "Test Course" } }
            }.BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseRatingStatsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(4.0, result.Data!.AverageRating); // (5+4+3)/3 = 4.0
            Assert.Equal(3, result.Data.TotalReviews);
        }

        [Fact]
        public async Task GetCourseRatingStatsAsync_CountsStarDistribution()
        {
            // Arrange
            var courseId = "course1";
            var reviews = new List<CourseReview>
            {
                new CourseReview { Id = "1", CourseId = courseId, Rating = 5, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "2", CourseId = courseId, Rating = 5, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "3", CourseId = courseId, Rating = 4, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "4", CourseId = courseId, Rating = 3, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "5", CourseId = courseId, Rating = 2, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "6", CourseId = courseId, Rating = 1, IsDeleted = false, Course = new Course { Name = "Test" } }
            }.BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseRatingStatsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.FiveStars);
            Assert.Equal(1, result.Data.FourStars);
            Assert.Equal(1, result.Data.ThreeStars);
            Assert.Equal(1, result.Data.TwoStars);
            Assert.Equal(1, result.Data.OneStar);
        }

        [Fact]
        public async Task GetCourseRatingStatsAsync_ReturnsZeroForNoReviews()
        {
            // Arrange
            var courseId = "course1";
            var reviews = new List<CourseReview>().BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseRatingStatsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.AverageRating);
            Assert.Equal(0, result.Data.TotalReviews);
        }

        [Fact]
        public async Task GetCourseRatingStatsAsync_IgnoresDeletedReviews()
        {
            // Arrange
            var courseId = "course1";
            var reviews = new List<CourseReview>
            {
                new CourseReview { Id = "1", CourseId = courseId, Rating = 5, IsDeleted = false, Course = new Course { Name = "Test" } },
                new CourseReview { Id = "2", CourseId = courseId, Rating = 1, IsDeleted = true, Course = new Course { Name = "Test" } }, // Should be ignored
                new CourseReview { Id = "3", CourseId = courseId, Rating = 5, IsDeleted = false, Course = new Course { Name = "Test" } }
            }.BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseRatingStatsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5.0, result.Data!.AverageRating); // (5+5)/2 = 5.0, ignoring deleted review
            Assert.Equal(2, result.Data.TotalReviews);
        }

        [Theory]
        [InlineData(new[] { 5, 5, 5, 5, 5 }, 5.0)]
        [InlineData(new[] { 1, 1, 1, 1, 1 }, 1.0)]
        [InlineData(new[] { 1, 2, 3, 4, 5 }, 3.0)]
        [InlineData(new[] { 5, 4 }, 4.5)]
        public async Task GetCourseRatingStatsAsync_CalculatesVariousAverages(int[] ratings, double expectedAverage)
        {
            // Arrange
            var courseId = "course1";
            var reviews = ratings.Select((rating, index) => new CourseReview
            {
                Id = $"review{index}",
                CourseId = courseId,
                Rating = rating,
                IsDeleted = false,
                Course = new Course { Name = "Test Course" }
            }).ToList().BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseRatingStatsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expectedAverage, result.Data!.AverageRating);
        }

        [Fact]
        public async Task GetCourseReviewsAsync_ReturnsOnlyNonDeletedReviews()
        {
            // Arrange
            var courseId = "course1";
            var reviews = new List<CourseReview>
            {
                new CourseReview { Id = "1", CourseId = courseId, IsDeleted = false, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() },
                new CourseReview { Id = "2", CourseId = courseId, IsDeleted = true, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() },
                new CourseReview { Id = "3", CourseId = courseId, IsDeleted = false, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() }
            }.BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetCourseReviewsAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count());
        }

        [Fact]
        public async Task GetStudentReviewsAsync_ReturnsOnlyStudentReviews()
        {
            // Arrange
            var studentId = "student1";
            var reviews = new List<CourseReview>
            {
                new CourseReview { Id = "1", StudentId = studentId, IsDeleted = false, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() },
                new CourseReview { Id = "2", StudentId = "other-student", IsDeleted = false, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() },
                new CourseReview { Id = "3", StudentId = studentId, IsDeleted = false, Student = new Domain.Entities.MainEntities.ApplicationUser(), Course = new Course() }
            }.BuildMockDbSet().Object;

            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.GetStudentReviewsAsync(studentId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count());
            Assert.All(result.Data, review => Assert.Equal(studentId, review.StudentId));
        }

        [Fact]
        public async Task ModerateReviewAsync_RemovesReviewWhenNotApproved()
        {
            // Arrange
            var reviewId = "review1";
            var review = new CourseReview
            {
                Id = reviewId,
                IsDeleted = false
            };

            var reviews = new List<CourseReview> { review }.BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ModerateReviewAsync(reviewId, approve: false);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("removed", result.Message);
            Assert.True(review.IsDeleted);
            Assert.NotNull(review.DeletedAt);
        }

        [Fact]
        public async Task ModerateReviewAsync_ApprovesReviewWhenApproved()
        {
            // Arrange
            var reviewId = "review1";
            var review = new CourseReview
            {
                Id = reviewId,
                IsDeleted = false
            };

            var reviews = new List<CourseReview> { review }.BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.ModerateReviewAsync(reviewId, approve: true);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("approved", result.Message);
            Assert.False(review.IsDeleted);
        }

        [Fact]
        public async Task ModerateReviewAsync_ReturnsErrorWhenReviewNotFound()
        {
            // Arrange
            var reviewId = "non-existent";
            var reviews = new List<CourseReview>().BuildMockDbSet().Object;
            _mockUnitOfWork.Setup(u => u.GetQueryable<CourseReview>()).Returns(reviews);

            // Act
            var result = await _service.ModerateReviewAsync(reviewId, approve: true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Review not found.", result.Message);
        }
    }
}
