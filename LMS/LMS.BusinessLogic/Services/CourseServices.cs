using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services.Helpers;
using LMS.DataAccess.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LMS.BusinessLogic.Services
{
    internal class CourseServices : BaseServices<Course, GetCourseDTO, CreateCourseDTO, UpdateCourseDTO>, ICourseServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public CourseServices(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }

        protected override IBaseRepository<Course, string> GetRepo() => _unitOfWork.Courses;

        protected override Course MapToEntity(CreateCourseDTO dto) => _mapper.Map<Course>(dto);

        protected override GetCourseDTO MapToReadDTO(Course entity)
        {
            var dto = _mapper.Map<GetCourseDTO>(entity);
            dto.AdminName = entity.Admin?.UserName
                ?? $"{entity.Admin?.FirstName} {entity.Admin?.LastName}";

            dto.CategoryName = entity.Category?.Name;
            if (!string.IsNullOrEmpty(dto.ThumbnailPath))
            {
                dto.ThumbnailPath = $"{dto.ThumbnailPath}";
            }

            dto.EnrolledStudentsCount = entity.Students?.Count ?? 0;

            if (entity.InstructorEnrollments != null)
            {
                dto.Instructors = entity.InstructorEnrollments
                    .Where(ie => ie.Status == ApplicationStatus.Approved)
                    .Select(ie => new InstructorInformationDTO
                    {
                        Id = ie.InstructorId,
                        Name = ie.Instructor?.UserName ?? $"{ie.Instructor?.FirstName} {ie.Instructor?.LastName}",
                        Email = ie.Instructor?.Email ?? "",
                        AssignedDays = new List<string>()
                    })
                    .ToList();
            }

            if (entity.Assignments != null)
            {
                dto.Assignments = entity.Assignments.Select(a => new LMS.BusinessLogic.DTOs.Assignment.ReadAssignmentDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    FilePath = a.FilePath,
                    UploadDate = a.UploadDate,
                    DueDate = a.DueDate,
                    CourseId = a.CourseId,
                    CourseName = entity.Name,
                    InstructorId = a.InstructorId,
                    InstructorName = a.Instructor?.UserName ?? $"{a.Instructor?.FirstName} {a.Instructor?.LastName}",
                    SubmissionsCount = a.Students?.Count ?? 0
                }).ToList();
            }
            if (entity.Quizzes != null)
            {
                dto.Quizzes = entity.Quizzes.Select(q => new LMS.BusinessLogic.DTOs.Quiz.ReadQuizDTO
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    NumberOfQuestions = q.NumberOfQuestions,
                    DurationMinutes = q.DurationMinutes,
                    PassingScore = q.PassingScore,
                    EndDate = q.EndDate,
                    CourseId = q.CourseId,
                    InstructorId = q.InstructorId
                }).ToList();
            }
            
            if (entity.LectureSchedules != null)
            {
                dto.Schedule = entity.LectureSchedules.Select(ls => _mapper.Map<LMS.BusinessLogic.DTOs.LectureSchedule.GetLectureScheduleDTO>(ls)).ToList();
            }

            return dto;
        }

        protected override Course UpdateToEntity(UpdateCourseDTO dto, Course existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Description = dto.Description;
            existingEntity.CourseCode = dto.CourseCode;
            existingEntity.Credits = dto.Credits;
            existingEntity.StartDate = dto.StartDate;
            existingEntity.EndDate = dto.EndDate;
            existingEntity.DurationWeeks = dto.DurationWeeks;
            existingEntity.Level = (Level)dto.Level;
            existingEntity.Language = dto.Language;
            existingEntity.DeliveryMode = dto.DeliveryMode;

            if (dto.IsFree != null && dto.IsFree == true)
            {
                existingEntity.Price = 0;
            }
            else
            {
                existingEntity.Price = dto.Price ?? 0;
            }

            existingEntity.IsFree = dto.IsFree;
            existingEntity.EveryStuCouldEnroll = dto.EveryStuCouldEnroll;
            existingEntity.MinAttendancePercentage = dto.MinAttendancePercentage;
            existingEntity.MinPerformanceScore = dto.MinPerformanceScore;
            existingEntity.AutoIssueCertificates = dto.AutoIssueCertificates;
            existingEntity.CertificateTemplateID = dto.CertificateTemplateId;
            existingEntity.CategoryId = dto.CategoryId;

            existingEntity.Status = CourseStatusHelper.DetermineCourseStatus(dto.StartDate, dto.EndDate);
            existingEntity.LastUpdate = DateTime.UtcNow;

            return existingEntity;
        }

        protected override string GetIdFromUpdateDTO(UpdateCourseDTO dto) => dto.Id;

        public async Task<ServiceResponseDTO<GetCourseDTO>> CreateCourse(CreateCourseDTO dto)
        {
            var response = new ServiceResponseDTO<GetCourseDTO>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (dto == null)
                {
                    response.Success = false;
                    response.Message = "Course data cannot be null.";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    response.Success = false;
                    response.Message = "Course name is required.";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(dto.CourseCode))
                {
                    response.Success = false;
                    response.Message = "Course code is required.";
                    return response;
                }

                if (dto.StartDate >= dto.EndDate)
                {
                    response.Success = false;
                    response.Message = "The start date must be before the end date.";
                    return response;
                }

                if (dto.StartDate < DateTime.UtcNow.Date)
                {
                    response.Success = false;
                    response.Message = "The start date must be in the future.";
                    return response;
                }

                var admin = await _unitOfWork.Users.FindByIdAsync(dto.AdminId);
                if (admin == null)
                {
                    response.Success = false;
                    response.Message = "Invalid AdminId. User not found.";
                    return response;
                }

                var category = await _unitOfWork.Categories.FindByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    response.Success = false;
                    response.Message = "Invalid CategoryId. Category not found.";
                    return response;
                }
                var existingCourse = await _unitOfWork.Courses.FindByCodeAsync(dto.CourseCode);
                if (existingCourse != null)
                {
                    response.Success = false;
                    response.Message = "A course with this code already exists.";
                    return response;
                }

                var course = _mapper.Map<Course>(dto);
                course.AdminId = admin.Id;
                course.CategoryId = category.Id;
                course.Status = CourseStatusHelper.DetermineCourseStatus(dto.StartDate, dto.EndDate);
                course.LastUpdate = DateTime.UtcNow;
                if (dto.IsFree)
                {
                    course.Price = 0;
                }
                else
                {
                    course.Price = dto.Price ?? 0;
                }
                if (dto.ThumbnailFile != null)
                {
                    var uploadResult = await _fileService.SaveCourseThumbnailAsync(dto.ThumbnailFile);

                    if (!uploadResult.Success)
                    {
                        response.Success = false;
                        response.Message = uploadResult.Message;
                        return response;
                    }

                    course.ThumbnailPath = uploadResult.FileName;
                }

                await _unitOfWork.Courses.CreateAsync(course);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdCourse = await _unitOfWork.Courses.FindByIdAsync(course.Id);
                var resultDto = MapToReadDTO(createdCourse);

                response.Success = true;
                response.Message = "Course created successfully.";
                response.Data = resultDto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponseDTO<GetCourseDTO>> UpdateCourseWithThumbnail(UpdateCourseDTO dto, IFormFile thumbnailFile)
        {
            var response = new ServiceResponseDTO<GetCourseDTO>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingCourse = await _unitOfWork.Courses.FindByIdAsync(dto.Id);
                if (existingCourse == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                // Check for duplicate course code
                if (existingCourse.CourseCode != dto.CourseCode)
                {
                    if (string.IsNullOrWhiteSpace(dto.CourseCode))
                    {
                        response.Success = false;
                        response.Message = "Course code cannot be empty.";
                        return response;
                    }

                    var duplicateCourse = await _unitOfWork.Courses.FindByCodeAsync(dto.CourseCode);
                    if (duplicateCourse != null)
                    {
                        response.Success = false;
                        response.Message = "A course with this code already exists.";
                        return response;
                    }
                }

                existingCourse = UpdateToEntity(dto, existingCourse);
                if (thumbnailFile != null)
                {
                    var uploadResult = await _fileService.SaveCourseThumbnailAsync(thumbnailFile);

                    if (!uploadResult.Success)
                    {
                        response.Success = false;
                        response.Message = uploadResult.Message;
                        return response;
                    }
                    if (!string.IsNullOrEmpty(existingCourse.ThumbnailPath))
                    {
                        await _fileService.DeleteCourseThumbnailAsync(existingCourse.ThumbnailPath);
                    }

                    existingCourse.ThumbnailPath = uploadResult.FileName;
                }

                await _unitOfWork.Courses.UpdateAsync(existingCourse);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                var updatedCourse = MapToReadDTO(existingCourse);
                response.Success = true;
                response.Message = "Course updated successfully.";
                response.Data = updatedCourse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error updating course: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponseDTO<GetCourseDTO>> UpdateThumbnailAsync(string courseId, IFormFile thumbnailFile)
        {
            var response = new ServiceResponseDTO<GetCourseDTO>();

            var course = await _unitOfWork.Courses.FindByIdAsync(courseId);
            if (course == null)
            {
                response.Success = false;
                response.Message = "Course not found";
                return response;
            }

            if (!string.IsNullOrWhiteSpace(course.ThumbnailPath))
                await _fileService.DeleteCourseThumbnailAsync(course.ThumbnailPath);

            var uploadResult = await _fileService.SaveCourseThumbnailAsync(thumbnailFile);
            if (!uploadResult.Success)
            {
                response.Success = false;
                response.Message = uploadResult.Message;
                return response;
            }

            course.ThumbnailPath = uploadResult.FileName;
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();
            response.Success = true;
                await _unitOfWork.SaveChangesAsync();
                response.Success = true;
                response.Message = "Thumbnail updated successfully";
                response.Data = MapToReadDTO(course);
                return response;
        }

        public override async Task<ServiceResponseDTO<GetCourseDTO>> GetByIdAsync(string id)
        {
            try
            {
                var entity = await GetRepo().GetQueryable()
                    .Include(c => c.Admin)
                    .Include(c => c.Category)
                    .Include(c => c.Students)
                    .Include(c => c.InstructorEnrollments).ThenInclude(ie => ie.Instructor)
                    .Include(c => c.Assignments).ThenInclude(a => a.Instructor)
                    .Include(c => c.Assignments).ThenInclude(a => a.Students)
                    .Include(c => c.Assignments).ThenInclude(a => a.Students)
                    .Include(c => c.Quizzes)
                    .Include(c => c.LectureSchedules)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (entity == null)
                {
                    return new ServiceResponseDTO<GetCourseDTO>
                    {
                        Success = false,
                        Message = "Entity Not Found."
                    };
                }

                var ReadEntity = MapToReadDTO(entity);
                return new ServiceResponseDTO<GetCourseDTO>
                {
                    Data = ReadEntity,
                    Success = true,
                    Message = "Entity retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity with id '{id}': {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<List<GetCourseDTO>>> GetPopularCoursesAsync(int count)
        {
            var response = new ServiceResponseDTO<List<GetCourseDTO>>();
            try
            {
                var courses = await _unitOfWork.Courses.GetAllAsync();
                var popularCourses = courses
                    .OrderByDescending(c => c.Students?.Count ?? 0)
                    .Take(count)
                    .ToList();

                var dtos = popularCourses.Select(c => MapToReadDTO(c)).ToList();

                response.Success = true;
                response.Data = dtos;
                response.Message = "Popular courses retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving popular courses: {ex.Message}";
            }
            return response;
        }
    }
}
