using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.DaySchedule;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.Services.Helpers;
using LMS.DataAcess.Contracts;
using LMS.Entity.Entities.MainEntities;
using LMS.Entity.Entities.RelationTables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CourseServices : BaseServices<Course, GetCourseDTO, CreateCourseDTO, UpdateCourseDTO>, ICourseServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public CourseServices(
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }

        public async Task<GetCourseDTO> CreateCourseWithScheduleAsync(CreateCourseDTO dto)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // ============ VALIDATION ============
                ValidateCreateCourseDTO(dto);

                // ============ HANDLE THUMBNAIL ============
                string thumbnailPath = null;
                if (dto.ThumbnailFile != null)
                {
                    thumbnailPath = await SaveThumbnailAsync(dto.ThumbnailFile);
                }

                // ============ CREATE COURSE ============
                var course = _mapper.Map<Course>(dto);
                course.ThumbnailPath = thumbnailPath;
                course.DeliveryMode = dto.DeliveryMode;
                course.Status = CourseStatusHelper.DetermineCourseStatus(dto.StartDate, dto.EndDate);
                course.LastUpdate = DateTime.UtcNow;

                await _unitOfWork.Coures.CreateAsync(course);
                await _unitOfWork.SaveChangesAsync();

                // ============ ADD DAY SCHEDULES ============
                var daySchedules = dto.DaySchedules
                    .Select(ds => new CourseDaySchedule
                    {
                        CourseId = course.Id,
                        DayOfWeek = ds.DayOfWeek,
                        StartTime = ds.StartTime,
                        EndTime = ds.EndTime,
                        InstructorId = ds.InstructorId,
                    })
                    .ToList();

                await _unitOfWork.DaySchedules.AddRangeAsync(daySchedules);
                await _unitOfWork.SaveChangesAsync();

                // ============ ADD INSTRUCTOR ENROLLMENTS ============
                var uniqueInstructorIds = dto.DaySchedules
                    .Select(ds => ds.InstructorId)
                    .Distinct();

                var instructorEnrollments = uniqueInstructorIds
                    .Select(instructorId => new InstructorEnrolltoCourse
                    {
                        InstructorId = instructorId,
                        CourseId = course.Id,
                        Status = ApplicationStatus.Approved,
                        ApprovedAt = DateTime.UtcNow,
                        RequestedAt = DateTime.UtcNow
                    })
                    .ToList();

                await _unitOfWork.InstructorEnrollments.AddRangeAsync(instructorEnrollments);
                await _unitOfWork.SaveChangesAsync();

                // ============ GENERATE LECTURES ============
                await GenerateLecturesAsync(course, daySchedules);

                // ✅ لو كل حاجة مشت تمام
                await transaction.CommitAsync();

                // ============ RETURN DTO ============
                var refreshedCourse = await _unitOfWork.Coures.FindByIdAsync(course.Id);
                return _mapper.Map<GetCourseDTO>(refreshedCourse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error creating course: {ex.Message}");
            }
        }

        public async Task<IEnumerable<GetLectureDTO>> GetCourseLecturesAsync(string courseId)
        {
            try
            {
                var lectures = await _unitOfWork.Lectures.GetCourseOcturesAsync(courseId);
                if (!lectures.Any())
                    return new List<GetLectureDTO>();

                return _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching lectures: {ex.Message}");
            }
        }

        public async Task<IEnumerable<GetDayScheduleDTO>> GetCourseScheduleAsync(string courseId)
        {
            try
            {
                var schedules = await _unitOfWork.DaySchedules.GetCourseSchedulesAsync(courseId);
                return _mapper.Map<IEnumerable<GetDayScheduleDTO>>(schedules);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching schedule: {ex.Message}");
            }
        }

        // ================== PRIVATE HELPERS ==================

        private void ValidateCreateCourseDTO(CreateCourseDTO dto)
        {
            if (dto.DaysPerWeek < 1 || dto.DaysPerWeek > 7)
                throw new Exception("Days per week must be between 1 and 7.");

            if (dto.SelectedDays?.Count != dto.DaysPerWeek)
                throw new Exception("The number of selected days does not match the number of days per week.");

            if (dto.DaySchedules?.Count == 0)
                throw new Exception("You must specify time slots and instructors for the sessions.");

            if (dto.DaySchedules.Any(ds => string.IsNullOrEmpty(ds.InstructorId)))
                throw new Exception("Each scheduled day must have an assigned instructor.");

            if (dto.StartDate >= dto.EndDate)
                throw new Exception("The start date must be before the end date.");

            if (dto.StartDate < DateTime.UtcNow.AddDays(-1))
                throw new Exception("The start date must be in the future.");
        }

        private async Task<string> SaveThumbnailAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thumbnails");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/thumbnails/{uniqueFileName}";
        }

        private async Task GenerateLecturesAsync(Course course, List<CourseDaySchedule> daySchedules)
        {
            var lectures = new List<Lecture>();
            var currentDate = course.StartDate;
            int lectureNumber = 1;

            while (lectureNumber <= course.TotalSessions)
            {
                int dayOfWeek = (int)currentDate.DayOfWeek;
                var schedule = daySchedules.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);

                if (schedule != null)
                {
                    var lecture = new Lecture
                    {
                        CourseId = course.Id,
                        LectureNumber = lectureNumber,
                        Title = $"Lecture #{lectureNumber}",
                        LectureDate = currentDate.Date,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        InstructorId = schedule.InstructorId,
                        CreatedAt = DateTime.UtcNow
                    };

                    lectures.Add(lecture);
                    lectureNumber++;
                }

                currentDate = currentDate.AddDays(1);
            }

            await _unitOfWork.Lectures.AddRangeAsync(lectures);
            await _unitOfWork.SaveChangesAsync();
        }

        // ================== BASE IMPLEMENTATIONS ==================

        protected override IBaseRepository<Course, string> GetRepo() => _unitOfWork.Coures;

        protected override Course MapToEntity(CreateCourseDTO dto) => _mapper.Map<Course>(dto);

        protected override Course UpdateToEntity(UpdateCourseDTO dto, Course existingEntity)
        {
            existingEntity.Name = dto.Name;
            existingEntity.Description = dto.Description;
            existingEntity.Credits = dto.Credits;
            existingEntity.StartDate = dto.StartDate;
            existingEntity.EndDate = dto.EndDate;
            existingEntity.DurationWeeks = dto.DurationWeeks;
            existingEntity.Price = dto.Price;
            existingEntity.IsFree = dto.IsFree;
            existingEntity.MinAttendancePercentage = dto.MinAttendancePercentage;
            existingEntity.MinPerformanceScore = dto.MinPerformanceScore;
            existingEntity.AutoIssueCertificates = dto.AutoIssueCertificates;
            existingEntity.DeliveryMode = dto.DeliveryMode;
            existingEntity.Status = CourseStatusHelper.DetermineCourseStatus(dto.StartDate, dto.EndDate);
            existingEntity.LastUpdate = DateTime.UtcNow;
            return existingEntity;
        }

        protected override GetCourseDTO MapToReadDTO(Course entity) => _mapper.Map<GetCourseDTO>(entity);

        protected override string GetIdFromUpdateDTO(UpdateCourseDTO dto) => dto.Id;
    }
}
