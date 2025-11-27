using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.CourseReview
{
    public class CreateCourseReviewDTO
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string CourseId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class ReadCourseReviewDTO
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UpdateCourseReviewDTO
    {
        [Required]
        public string Id { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class CourseRatingStatsDTO
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
    }
}
