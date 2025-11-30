using LMS.BusinessLogic.DTOs.Course;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using LMS.MVC.Models.ViewModels.Assignment;

namespace LMS.MVC.Services.Services
{
    public class CourseService : ICourseService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string ApiBase => "https://localhost:7033";
        private string ThumbnailEndpoint => $"{ApiBase}/uploads/course/thumbnails";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CourseService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Helpers

        private string ConvertThumbnail(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
            return $"{ThumbnailEndpoint}/{fileName}";
        }

        private void AttachTokenFromCookie()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                var token = context?.Request.Cookies["AccessToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<SuccessServiceResult<T>?> SendAndReadAsync<T>(Func<Task<HttpResponseMessage>> send)
        {
            AttachTokenFromCookie();

            HttpResponseMessage response;
            try
            {
                response = await send();
            }
            catch (HttpRequestException ex)
            {
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await ClearAuthStateAsync();
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = "Unauthorized. Please login again."
                };
            }

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var msg = !string.IsNullOrWhiteSpace(content) ? content : response.ReasonPhrase;
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = $"API error: {msg}"
                };
            }

            if (string.IsNullOrWhiteSpace(content) || content == "null")
            {
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = "API returned empty response"
                };
            }

            try
            {
                var result = JsonSerializer.Deserialize<SuccessServiceResult<T>>(content, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<T>
                {
                    Success = false,
                    Message = $"Failed to deserialize API response: {ex.Message}"
                };
            }
        }

        private async Task ClearAuthStateAsync()
        {
            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx is not null)
                {
                    ctx.Response.Cookies.Delete("AccessToken");
                    ctx.Response.Cookies.Delete("RefreshToken");
                    ctx.Response.Cookies.Delete("UserId");

                    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
            catch
            {
            }
        }

        #endregion

        public async Task<SuccessServiceResult<IEnumerable<ReadCourseResult>>> GetAllCoursesAsync()
        {
            var wrapper = await SendAndReadAsync<IEnumerable<ReadCourseResult>>(
                () => _httpClient.GetAsync($"{ApiBase}/api/courses/all")
            );

            if (wrapper?.Data != null)
            {
                foreach (var c in wrapper.Data)
                    c.ThumbnailPath = ConvertThumbnail(c.ThumbnailPath);
            }

            return wrapper ?? new SuccessServiceResult<IEnumerable<ReadCourseResult>>()
            {
                Success = false,
                Message = "Failed to load courses"
            };
        }

        public async Task<SuccessServiceResult<EditCourseViewModel>> GetCourseForEdit(Guid id)
        {
            var wrapper = await SendAndReadAsync<ReadCourseResult>(
                () => _httpClient.GetAsync($"{ApiBase}/api/courses/{id}")
            );

            if (wrapper?.Data == null || !wrapper.Success)
                return new SuccessServiceResult<EditCourseViewModel> { Success = false, Message = wrapper?.Message ?? "Course not found" };

            var data = wrapper.Data;

            var vm = new EditCourseViewModel
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Credits = data.Credits,
                Level = data.Level,
                Language = data.Language,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                DeliveryMode = data.DeliveryMode,
                Status = data.Status,
                Price = data.Price,
                IsFree = data.IsFree,
                EveryStuCouldEnroll = data.EveryStuCouldEnroll,
                AdminId = data.AdminId,
                CategoryId = data.CategoryId,
                CourseCode = data.CourseCode,
                ThumbnailPath = ConvertThumbnail(data.ThumbnailPath)
            };

            return new SuccessServiceResult<EditCourseViewModel>
            {
                Success = true,
                Data = vm
            };
        }

        public async Task<SuccessServiceResult<ReadCourseResult>> CreateCourse(CreateCourseViewModel vm)
        {
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(vm.Name ?? ""), "Name");
            formData.Add(new StringContent(vm.Description ?? ""), "Description");
            formData.Add(new StringContent(vm.CourseCode ?? ""), "CourseCode");
            formData.Add(new StringContent(vm.Credits.ToString()), "Credits");
            formData.Add(new StringContent(vm.Level.ToString()), "Level");
            formData.Add(new StringContent(vm.Language ?? ""), "Language");
            formData.Add(new StringContent(vm.StartDate.ToString("o")), "StartDate");
            formData.Add(new StringContent(vm.EndDate.ToString("o")), "EndDate");
            formData.Add(new StringContent(vm.DeliveryMode.ToString()), "DeliveryMode");
            formData.Add(new StringContent(vm.Status.ToString()), "Status");
            formData.Add(new StringContent(vm.Price?.ToString() ?? "0"), "Price");
            formData.Add(new StringContent(vm.IsFree.ToString()), "IsFree");
            formData.Add(new StringContent(vm.EveryStuCouldEnroll.ToString()), "EveryStuCouldEnroll");
            formData.Add(new StringContent(vm.AdminId ?? ""), "AdminId");
            formData.Add(new StringContent(vm.CategoryId ?? ""), "CategoryId");
            formData.Add(new StringContent(vm.AutoIssueCertificates.ToString()), "AutoIssueCertificates");
            formData.Add(new StringContent(vm.MinAttendancePercentage.ToString()), "MinAttendancePercentage");
            formData.Add(new StringContent(vm.MinPerformanceScore.ToString()), "MinPerformanceScore");

            if (!string.IsNullOrEmpty(vm.CertificateTemplateId))
                formData.Add(new StringContent(vm.CertificateTemplateId), "CertificateTemplateId");

            if (vm.ThumbnailFile != null)
            {
                var fileContent = new StreamContent(vm.ThumbnailFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(vm.ThumbnailFile.ContentType);
                formData.Add(fileContent, "ThumbnailFile", vm.ThumbnailFile.FileName);
            }

            var wrapper = await SendAndReadAsync<ReadCourseResult>(
                () => _httpClient.PostAsync($"{ApiBase}/api/courses/create", formData)
            );

            if (wrapper?.Data != null)
                wrapper.Data.ThumbnailPath = ConvertThumbnail(wrapper.Data.ThumbnailPath);

            return wrapper ?? new SuccessServiceResult<ReadCourseResult>()
            {
                Success = false,
                Message = "Failed to create course"
            };
        }

        public async Task<SuccessServiceResult<ReadCourseResult>> UpdateCourse(Guid id, EditCourseViewModel vm)
        {
            using var formData = new MultipartFormDataContent();

            // Add all form fields
            formData.Add(new StringContent(vm.Name ?? ""), nameof(vm.Name));
            formData.Add(new StringContent(vm.Description ?? ""), nameof(vm.Description));
            formData.Add(new StringContent(vm.CourseCode ?? ""), nameof(vm.CourseCode));
            formData.Add(new StringContent(vm.Credits.ToString()), nameof(vm.Credits));
            formData.Add(new StringContent(vm.Level.ToString()), nameof(vm.Level));
            formData.Add(new StringContent(vm.Language ?? ""), nameof(vm.Language));
            formData.Add(new StringContent(vm.StartDate.ToString("o")), nameof(vm.StartDate));
            formData.Add(new StringContent(vm.EndDate.ToString("o")), nameof(vm.EndDate));
            formData.Add(new StringContent(vm.DeliveryMode.ToString()), nameof(vm.DeliveryMode));
            formData.Add(new StringContent(vm.Status.ToString()), nameof(vm.Status));
            formData.Add(new StringContent(vm.Price?.ToString() ?? "0"), nameof(vm.Price));
            formData.Add(new StringContent(vm.IsFree.ToString()), nameof(vm.IsFree));
            formData.Add(new StringContent(vm.EveryStuCouldEnroll.ToString()), nameof(vm.EveryStuCouldEnroll));
            formData.Add(new StringContent(vm.AdminId ?? ""), nameof(vm.AdminId));
            formData.Add(new StringContent(vm.CategoryId ?? ""), nameof(vm.CategoryId));
            formData.Add(new StringContent(vm.AutoIssueCertificates.ToString()), nameof(vm.AutoIssueCertificates));
            formData.Add(new StringContent(vm.MinAttendancePercentage.ToString()), nameof(vm.MinAttendancePercentage));
            formData.Add(new StringContent(vm.MinPerformanceScore.ToString()), nameof(vm.MinPerformanceScore));
            formData.Add(new StringContent(vm.Id), nameof(vm.Id));

            if (!string.IsNullOrEmpty(vm.CertificateTemplateId))
                formData.Add(new StringContent(vm.CertificateTemplateId), nameof(vm.CertificateTemplateId));

            // Thumbnail file
            if (vm.ThumbnailFile != null)
            {
                var fileContent = new StreamContent(vm.ThumbnailFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(vm.ThumbnailFile.ContentType);
                formData.Add(fileContent, nameof(vm.ThumbnailFile), vm.ThumbnailFile.FileName);
            }

            // Call API
            var wrapper = await SendAndReadAsync<GetCourseDTO>(
                () => _httpClient.PutAsync($"{ApiBase}/api/courses/update/{id}", formData)
            );

            if (wrapper?.Data == null || !wrapper.Success)
            {
                return new SuccessServiceResult<ReadCourseResult>
                {
                    Success = false,
                    Message = wrapper?.Message ?? "Failed to update course"
                };
            }

            var data = wrapper.Data;
            var result = new ReadCourseResult
            {
                Id = data.Id,
                CourseCode = data.CourseCode,
                Name = data.Name,
                Description = data.Description,
                Credits = data.Credits,
                Level = data.Level,
                Language = data.Language,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                DurationWeeks = data.DurationWeeks,
                DeliveryMode = data.DeliveryMode,
                Status = data.Status,
                Price = data.Price,
                IsFree = data.IsFree,
                ThumbnailPath = ConvertThumbnail(data.ThumbnailPath),
                EveryStuCouldEnroll = data.EveryStuCouldEnroll,
                AdminId = data.AdminId,
                AdminName = data.AdminName,
                CategoryId = data.CategoryId,
                CategoryName = data.CategoryName,
                CertificateTemplateId = data.CertificateTemplateId,
                CertificateTemplateTitle = data.CertificateTemplateTitle,
                MinAttendancePercentage = data.MinAttendancePercentage,
                MinPerformanceScore = data.MinPerformanceScore,
                AutoIssueCertificates = data.AutoIssueCertificates,
                LastUpdate = data.LastUpdate,
                EnrolledStudentsCount = data.EnrolledStudentsCount,
                AverageRating = data.AverageRating,
                TotalSessions = data.TotalSessions,
                HoursPerSession = data.HoursPerSession,
                DaysPerWeek = data.DaysPerWeek,
                Schedule = data.Schedule,
                Instructors = data.Instructors
            };

            return new SuccessServiceResult<ReadCourseResult>
            {
                Success = true,
                Message = wrapper.Message,
                Data = result
            };
        }


        public async Task<SuccessServiceResult<ReadCourseViewModel>> GetCourseDetails(Guid id)
        {
            var wrapper = await SendAndReadAsync<GetCourseDTO>(
                () => _httpClient.GetAsync($"{ApiBase}/api/courses/{id}")
            );

            if (wrapper?.Data == null || !wrapper.Success)
                return new SuccessServiceResult<ReadCourseViewModel>
                {
                    Success = false,
                    Message = wrapper?.Message ?? "Course not found"
                };

            var data = wrapper.Data;
            var vm = new ReadCourseViewModel
            {
                Id = data.Id,
                Name = data.Name,
                CourseCode = data.CourseCode,
                Description = data.Description,
                Credits = data.Credits,
                Level = data.Level,
                Language = data.Language,
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                DeliveryMode = data.DeliveryMode,
                Status = data.Status,
                Price = data.Price,
                IsFree = data.IsFree,
                EveryStuCouldEnroll = data.EveryStuCouldEnroll,
                AdminId = data.AdminId,
                AdminName = data.AdminName,
                CategoryId = data.CategoryId,
                CategoryName = data.CategoryName,
                CertificateTemplateId = data.CertificateTemplateId,
                CertificateTemplateTitle = data.CertificateTemplateTitle,
                MinAttendancePercentage = data.MinAttendancePercentage,
                MinPerformanceScore = data.MinPerformanceScore,
                AutoIssueCertificates = data.AutoIssueCertificates,
                LastUpdate = data.LastUpdate,
                EnrolledStudentsCount = data.EnrolledStudentsCount,
                AverageRating = data.AverageRating,
                TotalSessions = data.TotalSessions,
                HoursPerSession = data.HoursPerSession,
                DaysPerWeek = data.DaysPerWeek,
                Schedule = data.Schedule,
                Instructors = data.Instructors,
                ThumbnailPath = ConvertThumbnail(data.ThumbnailPath),
                Assignments = data.Assignments?.Select(a => new ReadAssignmentResult
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    CourseId = a.CourseId,
                    CourseName = a.CourseName,
                    InstructorId = a.InstructorId,
                    InstructorName = a.InstructorName
                }).ToList() ?? new List<ReadAssignmentResult>(),
                Quizzes = data.Quizzes?.Select(q => new LMS.MVC.Models.ViewModels.Quiz.QuizItemViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    NumberOfQuestions = q.NumberOfQuestions,
                    DurationMinutes = q.DurationMinutes,
                    PassingScore = q.PassingScore,
                    EndDate = q.EndDate,
                    IsCompleted = false // Logic for this would be complex, defaulting to false for now
                }).ToList() ?? new List<LMS.MVC.Models.ViewModels.Quiz.QuizItemViewModel>()
            };

            return new SuccessServiceResult<ReadCourseViewModel>
            {
                Success = true,
                Data = vm
            };
        }



        public async Task<bool> DeleteCourse(Guid id)
        {
            var wrapper = await SendAndReadAsync<object>(
                () => _httpClient.DeleteAsync($"{ApiBase}/api/courses/delete/{id}")
            );

            return wrapper?.Success == true;
        }

        public async Task<bool> IsUserEnrollIntoCourse(string id, string courseId)
        {
            AttachTokenFromCookie();
            var model = new
            {
                UserId = id,
                CourseId = courseId
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var wrapper = await SendAndReadAsync<string>(
                () => _httpClient.PostAsync($"{ApiBase}/api/courses/isenrolled", jsonContent)
            );

            if (wrapper == null || !wrapper.Success || wrapper.Data == null)
                return false;

            return bool.TryParse(wrapper.Data, out var b) && b;
        }

    }
}
