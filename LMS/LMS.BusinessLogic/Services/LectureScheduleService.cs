using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    public class LectureScheduleService : ILectureScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LectureScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> CreateScheduleAsync(CreateLectureScheduleDTO dto, string userId, string userRole)
        {
            try
            {
                // Validate permissions (Admin only)
                if (userRole != "Admin")
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO>
                    {
                        Success = false,
                        Message = "Only admins can create lecture schedules."
                    };
                }

                // Validate course exists
                var course = await _unitOfWork.Courses.FindByIdAsync(dto.CourseId);
                if (course == null)
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO>
                    {
                        Success = false,
                        Message = "Course not found."
                    };
                }

                // Create schedules for each selected day
                var createdSchedules = new List<GetLectureScheduleDTO>();
                
                foreach (var scheduleDto in dto.Schedules)
                {
                    // Check if schedule already exists for this day
                    var existingSchedules = await _unitOfWork.LectureSchedules.GetByCourseIdAsync(dto.CourseId);
                    if (existingSchedules.Any(s => s.DayOfWeek == scheduleDto.DayOfWeek))
                    {
                        continue; // Skip if already exists
                    }

                    var schedule = _mapper.Map<LectureSchedule>(scheduleDto);
                    schedule.CourseId = dto.CourseId;
                    schedule.Title = dto.Title;
                    schedule.Description = dto.Description;

                    await _unitOfWork.LectureSchedules.CreateAsync(schedule);
                    await _unitOfWork.SaveChangesAsync();

                    // Generate lectures for this schedule
                    await GenerateLecturesForScheduleInternalAsync(schedule);
                    
                    createdSchedules.Add(_mapper.Map<GetLectureScheduleDTO>(schedule));
                }

                if (!createdSchedules.Any())
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO>
                    {
                        Success = false,
                        Message = "No new schedules were created. Schedules may already exist for the selected days."
                    };
                }

                // Return the first created schedule as representative, or modify DTO to return list
                return new ServiceResponseDTO<GetLectureScheduleDTO>
                {
                    Success = true,
                    Data = createdSchedules.First(),
                    Message = $"Successfully created {createdSchedules.Count} schedules and generated lectures."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<GetLectureScheduleDTO>
                {
                    Success = false,
                    Message = $"Error creating schedule: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<bool>> DeleteScheduleAsync(string id, string userId, string userRole)
        {
            try
            {
                if (userRole != "Admin")
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Unauthorized" };
                }

                var schedule = await _unitOfWork.LectureSchedules.GetByIdWithLecturesAsync(id);
                if (schedule == null)
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Schedule not found" };
                }

                // Soft delete schedule
                await _unitOfWork.LectureSchedules.DeleteByEntityAsync(schedule);
                
                // Also soft delete associated lectures
                foreach (var lecture in schedule.Lectures)
                {
                    await _unitOfWork.Lectures.DeleteByEntityAsync(lecture);
                }

                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Schedule and associated lectures deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponseDTO<bool>> GenerateLecturesFromScheduleAsync(string scheduleId, string userId, string userRole)
        {
            try
            {
                if (userRole != "Admin")
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Unauthorized" };
                }

                var schedule = await _unitOfWork.LectureSchedules.FindByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Schedule not found" };
                }

                await GenerateLecturesForScheduleInternalAsync(schedule);

                return new ServiceResponseDTO<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Lectures generated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>> GetCourseSchedulesAsync(string courseId, string userId, string userRole)
        {
            try
            {
                var schedules = await _unitOfWork.LectureSchedules.GetByCourseIdWithLecturesAsync(courseId);
                var dtos = _mapper.Map<IEnumerable<GetLectureScheduleDTO>>(schedules);
                
                // Manually map generated lectures count if not handled by automapper
                var scheduleList = schedules.ToList();
                var dtoList = dtos.ToList();
                
                for (int i = 0; i < scheduleList.Count; i++)
                {
                    dtoList[i].GeneratedLecturesCount = scheduleList[i].Lectures.Count(l => !l.IsDeleted);
                }

                return new ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>>
                {
                    Success = true,
                    Data = dtoList
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<GetLectureScheduleDTO>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> GetScheduleByIdAsync(string id, string userId, string userRole)
        {
            try
            {
                var schedule = await _unitOfWork.LectureSchedules.GetByIdWithLecturesAsync(id);
                if (schedule == null)
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO> { Success = false, Message = "Schedule not found" };
                }

                var dto = _mapper.Map<GetLectureScheduleDTO>(schedule);
                dto.GeneratedLecturesCount = schedule.Lectures.Count(l => !l.IsDeleted);

                return new ServiceResponseDTO<GetLectureScheduleDTO>
                {
                    Success = true,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<GetLectureScheduleDTO> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponseDTO<GetLectureScheduleDTO>> UpdateScheduleAsync(UpdateLectureScheduleDTO dto, string userId, string userRole)
        {
            try
            {
                if (userRole != "Admin")
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO> { Success = false, Message = "Unauthorized" };
                }

                var schedule = await _unitOfWork.LectureSchedules.GetByIdWithLecturesAsync(dto.Id);
                if (schedule == null)
                {
                    return new ServiceResponseDTO<GetLectureScheduleDTO> { Success = false, Message = "Schedule not found" };
                }

                // Check if DayOfWeek changed
                bool dayChanged = schedule.DayOfWeek != dto.DayOfWeek;

                // Update properties
                schedule.Title = dto.Title;
                schedule.Description = dto.Description;
                schedule.DayOfWeek = dto.DayOfWeek;
                schedule.StartTime = dto.StartTime;
                schedule.DurationMinutes = dto.DurationMinutes;

                await _unitOfWork.LectureSchedules.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                // Propagate changes to future lectures
                var today = DateTime.Today;
                var futureLectures = schedule.Lectures.Where(l => l.LectureDate >= today && !l.IsDeleted).ToList();

                if (dayChanged)
                {
                    // If day changed, delete future lectures and regenerate
                    foreach (var lecture in futureLectures)
                    {
                        await _unitOfWork.Lectures.DeleteByEntityAsync(lecture);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    await GenerateLecturesForScheduleInternalAsync(schedule, DateTime.Today);
                }
                else
                {
                    // If day didn't change, update existing future lectures
                    foreach (var lecture in futureLectures)
                    {
                        lecture.Title = $"{schedule.Title} {lecture.LectureNumber}";
                        lecture.Description = schedule.Description;
                        lecture.StartTime = schedule.StartTime;
                        lecture.EndTime = schedule.StartTime.Add(TimeSpan.FromMinutes(schedule.DurationMinutes));
                        
                        await _unitOfWork.Lectures.UpdateAsync(lecture);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                return new ServiceResponseDTO<GetLectureScheduleDTO>
                {
                    Success = true,
                    Data = _mapper.Map<GetLectureScheduleDTO>(schedule),
                    Message = "Schedule updated and future lectures synchronized successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<GetLectureScheduleDTO> { Success = false, Message = ex.Message };
            }
        }

        private async Task GenerateLecturesForScheduleInternalAsync(LectureSchedule schedule, DateTime? fromDate = null)
        {
            var course = await _unitOfWork.Courses.FindByIdAsync(schedule.CourseId);
            if (course == null) return;

            // Calculate dates
            var currentDate = fromDate ?? course.StartDate;
            var endDate = course.EndDate;

            // Find the first occurrence of the scheduled day
            while (currentDate.DayOfWeek != schedule.DayOfWeek)
            {
                currentDate = currentDate.AddDays(1);
            }

            // Generate lectures until end date
            var lectures = new List<Lecture>();
            int lectureNumber = 1;

            // Get existing max lecture number
            var existingLectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(schedule.CourseId);
            if (existingLectures.Any())
            {
                lectureNumber = existingLectures.Max(l => l.LectureNumber) + 1;
            }

            while (currentDate <= endDate)
            {
                // Check if lecture already exists for this date (to prevent duplicates)
                bool exists = existingLectures.Any(l => l.LectureDate.Date == currentDate.Date && !l.IsDeleted);
                
                if (!exists)
                {
                    var lecture = new Lecture
                    {
                        CourseId = schedule.CourseId,
                        LectureScheduleId = schedule.Id,
                        Title = $"{schedule.Title} {lectureNumber}",
                        Description = schedule.Description,
                        LectureDate = currentDate,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.StartTime.Add(TimeSpan.FromMinutes(schedule.DurationMinutes)),
                        LectureNumber = lectureNumber,
                        InstructorId = schedule.InstructorId // Use assigned instructor
                    };
                    lectures.Add(lecture);
                    lectureNumber++;
                }

                currentDate = currentDate.AddDays(7);
            }

            if (lectures.Any())
            {
                await _unitOfWork.Lectures.AddRangeAsync(lectures);
                await _unitOfWork.SaveChangesAsync();
            }

            // Re-number all lectures for the course to ensure chronological order
            await RenumberCourseLecturesAsync(schedule.CourseId);
        }

        private async Task RenumberCourseLecturesAsync(string courseId)
        {
            var allLectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseId);
            var sortedLectures = allLectures.OrderBy(l => l.LectureDate).ThenBy(l => l.StartTime).ToList();

            for (int i = 0; i < sortedLectures.Count; i++)
            {
                var lecture = sortedLectures[i];
                var newNumber = i + 1;

                if (lecture.LectureNumber != newNumber)
                {
                    lecture.LectureNumber = newNumber;
                    
                    // Update title if it follows the standard format "{Title} {Number}"
                    // We assume the last part is the number. This is a heuristic.
                    // Better approach: If we have the schedule, use its title.
                    // Since we might not have the schedule loaded here easily for all, 
                    // we can try to reconstruct it or just update the number at the end.
                    
                    if (!string.IsNullOrEmpty(lecture.Title))
                    {
                        var lastSpaceIndex = lecture.Title.LastIndexOf(' ');
                        if (lastSpaceIndex > 0)
                        {
                            var baseTitle = lecture.Title.Substring(0, lastSpaceIndex);
                            lecture.Title = $"{baseTitle} {newNumber}";
                        }
                    }
                    
                    await _unitOfWork.Lectures.UpdateAsync(lecture);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
