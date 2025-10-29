using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.DaySchedule;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts.Services;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Services
{
    public class CourseService : ICourseService
    {
        private readonly HttpClient _client;
        private readonly ITokenService _tokenService;

        public CourseService(HttpClient client, ITokenService tokenService)
        {
            _client = client;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<AllCoursesResult>> GetAllCoursesAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync("api/courses/all");

            if (response.IsSuccessStatusCode)
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<IEnumerable<GetCourseDTO>>>();

                var dtoCourses = serviceResponse?.Data ?? new List<GetCourseDTO>();

                var result = dtoCourses.Select(dto => new AllCoursesResult
                {
                    Id = dto.Id,
                    CourseCode = dto.CourseCode,
                    Name = dto.Name,
                    Description = dto.Description,
                    Credits = dto.Credits,
                    Level = dto.Level,
                    Language = dto.Language,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    DurationWeeks = dto.DurationWeeks,
                    DeliveryMode = dto.DeliveryMode,
                    Status = dto.Status,
                    Price = dto.Price,
                    IsFree = dto.IsFree,
                    ThumbnailPath = dto.ThumbnailPath,
                    AdminId = dto.AdminId,
                    AdminName = dto.AdminName,
                    CategoryId = dto.CategoryId,
                    CategoryName = dto.CategoryName,
                    CertificateTemplateId = dto.CertificateTemplateId,
                    CertificateTemplateTitle = dto.CertificateTemplateTitle,
                    MinAttendancePercentage = dto.MinAttendancePercentage,
                    MinPerformanceScore = dto.MinPerformanceScore,
                    AutoIssueCertificates = dto.AutoIssueCertificates,
                    LastUpdate = dto.LastUpdate,
                    EnrolledStudentsCount = dto.EnrolledStudentsCount,
                    AverageRating = dto.AverageRating,
                    TotalSessions = dto.TotalSessions,
                    HoursPerSession = dto.HoursPerSession,
                    DaysPerWeek = dto.DaysPerWeek,
                    Schedule = dto.Schedule?.ToList() ?? new List<CreateDayScheduleDTO>(),
                    Instructors = dto.Instructors?.ToList() ?? new List<InstructorInformationDTO>()
                }).ToList();


                return result;
            }

            return new List<AllCoursesResult>();
        }


        //public async Task<ReadCourseDTO?> GetCourseByIdAsync(string id)
        //{
        //    var token = await _tokenService.GetAccessTokenAsync();
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    }

        //    var response = await _client.GetAsync($"api/course/{id}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var course = await response.Content.ReadFromJsonAsync<ReadCourseDTO>();
        //        return course;
        //    }

        //    return null;
        //}
    }
}
