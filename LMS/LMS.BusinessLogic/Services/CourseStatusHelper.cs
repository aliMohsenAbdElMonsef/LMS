using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Course;
using LMS.DataAcess.Contracts;
using LMS.Entity.Entities.MainEntities;
using Microsoft.AspNetCore.Hosting;

namespace LMS.BusinessLogic.Services.Helpers
{
    public static class CourseStatusHelper
    {
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IMapper _mapper;

        public static Status DetermineCourseStatus(DateTime startDate, DateTime endDate, DateTime currentDate)
        {
            if (currentDate < startDate)
                return Status.Draft;

            if (currentDate >= startDate && currentDate <= endDate)
                return Status.Ongoing;

            if (currentDate > endDate)
                return Status.Completed;

            return Status.Draft;
        }

        public static Status DetermineCourseStatus(DateTime startDate, DateTime endDate)
        {
            return DetermineCourseStatus(startDate, endDate, DateTime.UtcNow);
        }
        public static string GetStatusDescription(Status status)
        {
            return status switch
            {
                Status.Draft => "Draft - Not yet published",
                Status.Published => "Published - Ready to start",
                Status.Ongoing => "Ongoing - Currently running",
                Status.Completed => "Completed - Finished",
                Status.Archived => "Archived - Stored",
                _ => "Unknown"
            };
        }


        public static string GetDeliveryModeDescription(DeliveryMode mode)
        {
            return mode switch
            {
                DeliveryMode.Online => "Online",
                DeliveryMode.Onsite => "OnSite",
                DeliveryMode.Hybrid => "Hybrid(Online+Onsite)"
            };
        }
    }
}

//        private static void ValidateCreateCourseDTO(CreateCourseDTO dto)
//        {
//            if (dto.DaysPerWeek < 1 || dto.DaysPerWeek > 7)
//                throw new Exception("Days per week must be between 1 and 7.");

//            if (dto.SelectedDays?.Count != dto.DaysPerWeek)
//                throw new Exception("The number of selected days does not match the number of days per week.");

//            if (dto.DaySchedules?.Count == 0)
//                throw new Exception("You must specify time slots and instructors for the sessions.");

//            if (dto.DaySchedules.Any(ds => string.IsNullOrEmpty(ds.InstructorId)))
//                throw new Exception("Each scheduled day must have an assigned instructor.");

//            if (dto.StartDate >= dto.EndDate)
//                throw new Exception("The start date must be before the end date.");

//            if (dto.StartDate < DateTime.UtcNow.AddDays(-1))
//                throw new Exception("The start date must be in the future.");
//        }

//        private static async Task GenerateLecturesAsync(Course course, List<CourseDaySchedule> daySchedules)
//        {
//            var lectures = new List<Lecture>();
//            var currentDate = course.StartDate;
//            int lectureNumber = 1;

//            while (lectureNumber <= course.TotalSessions)
//            {
//                int dayOfWeek = (int)currentDate.DayOfWeek;
//                var schedule = daySchedules.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);

//                if (schedule != null)
//                {
//                    var lecture = new Lecture
//                    {
//                        CourseId = course.Id,
//                        LectureNumber = lectureNumber,
//                        Title = $"Lecture #{lectureNumber}",
//                        LectureDate = currentDate.Date,
//                        StartTime = schedule.StartTime,
//                        EndTime = schedule.EndTime,
//                        InstructorId = schedule.InstructorId,
//                        CreatedAt = DateTime.UtcNow
//                    };

//                    lectures.Add(lecture);
//                    lectureNumber++;
//                }

//                currentDate = currentDate.AddDays(1);
//            }

//            await _unitOfWork.Lectures.AddRangeAsync(lectures);
//            await _unitOfWork.SaveChangesAsync();
//        }


//        private static async Task<string> SaveThumbnailAsync(Microsoft.AspNetCore.Http.IFormFile file)
//        {
//            try
//            {
//                if (file?.Length == 0)
//                    return null;

//                var uploadsFolder = System.IO.Path.Combine(
//                    _webHostEnvironment.WebRootPath, "uploads", "thumbnails");

//                if (!System.IO.Directory.Exists(uploadsFolder))
//                    System.IO.Directory.CreateDirectory(uploadsFolder);

//                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
//                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

//                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
//                {
//                    await file.CopyToAsync(fileStream);
//                }

//                return $"/uploads/thumbnails/{uniqueFileName}";
//            }
//            catch
//            {
//                return null;
//            }
//        }
//    }
//}

    
