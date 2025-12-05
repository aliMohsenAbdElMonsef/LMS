using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CourseReview;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CourseReviewService : ICourseReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponseDTO<ReadCourseReviewDTO>> CreateReviewAsync(CreateCourseReviewDTO dto)
        {
            try
            {

                var existingReview = await _unitOfWork.GetQueryable<CourseReview>()
                    .FirstOrDefaultAsync(r => r.StudentId == dto.StudentId && r.CourseId == dto.CourseId && !r.IsDeleted);

                if (existingReview != null)
                {
                    return new ServiceResponseDTO<ReadCourseReviewDTO>
                    {
                        Success = false,
                        Message = "You have already reviewed this course. Please update your existing review."
                    };
                }

                var review = new CourseReview
                {
                    Id = Guid.NewGuid().ToString(),
                    StudentId = dto.StudentId,
                    CourseId = dto.CourseId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };


                await _unitOfWork.SaveChangesAsync();

                var readDto = await GetReviewByIdAsync(review.Id);
                return readDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating review: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<ReadCourseReviewDTO>> UpdateReviewAsync(UpdateCourseReviewDTO dto)
        {
            try
            {
                var review = await _unitOfWork.GetQueryable<CourseReview>()
                    .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

                if (review == null)
                {
                    return new ServiceResponseDTO<ReadCourseReviewDTO>
                    {
                        Success = false,
                        Message = "Review not found."
                    };
                }

                if (dto.Rating.HasValue)
                    review.Rating = dto.Rating.Value;

                if (dto.Comment != null)
                    review.Comment = dto.Comment;

                await _unitOfWork.SaveChangesAsync();

                return await GetReviewByIdAsync(review.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating review: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<ReadCourseReviewDTO>> DeleteReviewAsync(string reviewId)
        {
            try
            {
                var review = await _unitOfWork.GetQueryable<CourseReview>()
                    .FirstOrDefaultAsync(r => r.Id == reviewId);

                if (review == null)
                {
                    return new ServiceResponseDTO<ReadCourseReviewDTO>
                    {
                        Success = false,
                        Message = "Review not found."
                    };
                }

                review.IsDeleted = true;
                review.DeletedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<ReadCourseReviewDTO>
                {
                    Success = true,
                    Message = "Review deleted successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting review: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<ReadCourseReviewDTO>> GetReviewByIdAsync(string reviewId)
        {
            try
            {
                var review = await _unitOfWork.GetQueryable<CourseReview>()
                    .Include(r => r.Student)
                    .Include(r => r.Course)
                    .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

                if (review == null)
                {
                    return new ServiceResponseDTO<ReadCourseReviewDTO>
                    {
                        Success = false,
                        Message = "Review not found."
                    };
                }

                return new ServiceResponseDTO<ReadCourseReviewDTO>
                {
                    Data = MapToReadDTO(review),
                    Success = true,
                    Message = "Review retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving review: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>> GetCourseReviewsAsync(string courseId)
        {
            try
            {
                var reviews = await _unitOfWork.GetQueryable<CourseReview>()
                    .Include(r => r.Student)
                    .Include(r => r.Course)
                    .Where(r => r.CourseId == courseId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return new ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>
                {
                    Data = reviews.Select(MapToReadDTO),
                    Success = true,
                    Message = "Reviews retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving course reviews: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>> GetStudentReviewsAsync(string studentId)
        {
            try
            {
                var reviews = await _unitOfWork.GetQueryable<CourseReview>()
                    .Include(r => r.Student)
                    .Include(r => r.Course)
                    .Where(r => r.StudentId == studentId && !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return new ServiceResponseDTO<IEnumerable<ReadCourseReviewDTO>>
                {
                    Data = reviews.Select(MapToReadDTO),
                    Success = true,
                    Message = "Reviews retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student reviews: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<CourseRatingStatsDTO>> GetCourseRatingStatsAsync(string courseId)
        {
            try
            {
                var reviews = await _unitOfWork.GetQueryable<CourseReview>()
                    .Include(r => r.Course)
                    .Where(r => r.CourseId == courseId && !r.IsDeleted)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return new ServiceResponseDTO<CourseRatingStatsDTO>
                    {
                        Data = new CourseRatingStatsDTO
                        {
                            CourseId = courseId,
                            CourseName = reviews.FirstOrDefault()?.Course?.Name ?? "",
                            AverageRating = 0,
                            TotalReviews = 0
                        },
                        Success = true,
                        Message = "No reviews found for this course."
                    };
                }

                var stats = new CourseRatingStatsDTO
                {
                    CourseId = courseId,
                    CourseName = reviews.First().Course.Name,
                    AverageRating = reviews.Average(r => r.Rating),
                    TotalReviews = reviews.Count,
                    FiveStars = reviews.Count(r => r.Rating == 5),
                    FourStars = reviews.Count(r => r.Rating == 4),
                    ThreeStars = reviews.Count(r => r.Rating == 3),
                    TwoStars = reviews.Count(r => r.Rating == 2),
                    OneStar = reviews.Count(r => r.Rating == 1)
                };

                return new ServiceResponseDTO<CourseRatingStatsDTO>
                {
                    Data = stats,
                    Success = true,
                    Message = "Rating statistics retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving rating stats: {ex.Message}", ex);
            }
        }

        public async Task<BasicResponseDTO> ModerateReviewAsync(string reviewId, bool approve)
        {
            try
            {
                var review = await _unitOfWork.GetQueryable<CourseReview>()
                    .FirstOrDefaultAsync(r => r.Id == reviewId);

                if (review == null)
                {
                    return new BasicResponseDTO
                    {
                        Success = false,
                        Message = "Review not found."
                    };
                }

                if (!approve)
                {
                    review.IsDeleted = true;
                    review.DeletedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                return new BasicResponseDTO
                {
                    Success = true,
                    Message = approve ? "Review approved." : "Review removed."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error moderating review: {ex.Message}", ex);
            }
        }

        private ReadCourseReviewDTO MapToReadDTO(CourseReview review)
        {
            return new ReadCourseReviewDTO
            {
                Id = review.Id,
                StudentId = review.StudentId,
                StudentName = review.Student != null ? $"{review.Student.FirstName} {review.Student.LastName}" : "",
                CourseId = review.CourseId,
                CourseName = review.Course?.Name ?? "",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                IsDeleted = review.IsDeleted
            };
        }
    }
}
