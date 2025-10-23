using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
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
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CourseServices:BaseServices<Course, GetCourseDTO, CreateCourseDTO, UpdateCourseDTO>, ICourseServices
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
                throw new Exception($"خطأ في جلب الحصص: {ex.Message}");
            }
        }


    }
}
