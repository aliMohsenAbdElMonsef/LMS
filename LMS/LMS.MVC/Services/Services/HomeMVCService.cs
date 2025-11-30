using AutoMapper;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    internal class HomeMVCService : BaseMVCServices, IHomeMVCService
    {
        private readonly IMapper _mapper;
        private string ApiBase => "https://localhost:7033";
        private string ThumbnailEndpoint => $"{ApiBase}/uploads/course/thumbnails";

        public HomeMVCService(
            HttpClient client,
            IHttpContextAccessor contextAccessor,
            IMapper mapper)
            : base(client, contextAccessor)
        {
            _mapper = mapper;
        }

        private string ConvertThumbnail(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "/images/default-course.jpg";
            return $"{ThumbnailEndpoint}/{fileName}";
        }

        public async Task<List<ReadCategoryResult>> GetTopCategoriesAsync(int count = 6)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<IEnumerable<ReadCategoryDTO>>>(
                    $"{ApiBase}/api/category/top?count={count}");

                if (response?.Success == true && response.Data != null)
                {
                    return _mapper.Map<List<ReadCategoryResult>>(response.Data);
                }

                return new List<ReadCategoryResult>();
            }
            catch (Exception)
            {
                return new List<ReadCategoryResult>();
            }
        }

        public async Task<List<ReadCourseResult>> GetPopularCoursesAsync(int count = 6)
        {
            try
            {
                var response = await GetAsync<ServiceResponseDTO<IEnumerable<GetCourseDTO>>>(
                    $"{ApiBase}/api/courses/popular?count={count}");

                if (response?.Success == true && response.Data != null)
                {
                    var courses = _mapper.Map<List<ReadCourseResult>>(response.Data);

                    // Convert thumbnail paths
                    foreach (var course in courses)
                    {
                        course.ThumbnailPath = ConvertThumbnail(course.ThumbnailPath);
                    }

                    return courses;
                }

                return new List<ReadCourseResult>();
            }
            catch (Exception)
            {
                return new List<ReadCourseResult>();
            }
        }
    }
}