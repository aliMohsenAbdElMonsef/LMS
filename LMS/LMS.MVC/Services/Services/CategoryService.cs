using AutoMapper;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts.Services;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace LMS.MVC.Services.Services
{
    internal class CategoryService : BaseMVCServices, ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public CategoryService(
            HttpClient client,
            IHttpContextAccessor contextAccessor,
            IMapper mapper,
            ITokenService tokenService)
            : base(client, contextAccessor)
        {
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<ReadCategoryResult>> GetAllCategories()
        {
            var serviceResponse = await GetAsync<ServiceResponseDTO<IEnumerable<ReadCategoryDTO>>>("api/category/all");

            var categories = serviceResponse?.Data ?? Enumerable.Empty<ReadCategoryDTO>();

            return _mapper.Map<IEnumerable<ReadCategoryResult>>(categories);
        }

        public async Task<CategoryDetailsResult> GetCategoryById(string id)
        {
            var response = await GetAsync<ServiceResponseDTO<CategoryDetailsDTO>>($"api/category/details/{id}");

            // Check if API call was successful
            if (response == null || !response.Success || response.Data == null)
                return null;

            var category = response.Data;

            return new CategoryDetailsResult
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreationDate = category.CreationDate,
                LastUpdated = category.LastUpdated,
                CoursesCount = category.CoursesCount,
                Courses = _mapper.Map<List<ReadCourseResult>>(category.Courses)
            };
        }

        public async Task<ReadCategoryResult> GetEditModel(string id)
        {
            var response = await GetAsync<ServiceResponseDTO<ReadCategoryDTO>>($"api/category/{id}");

            // Check if API call was successful
            if (response == null || !response.Success || response.Data == null)
                return null;

            return _mapper.Map<ReadCategoryResult>(response.Data);
        }


        public async Task<T> PutAsync<T>(string url, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseJson) || responseJson == "null")
                throw new Exception($"API returned null for PUT {url}");
            return JsonConvert.DeserializeObject<T>(responseJson)!;
        }




        public async Task<ReadCategoryResult> EditCategory(ReadCategoryResult model)
        {
            if (string.IsNullOrEmpty(model.AdminId))
                model.AdminId = _tokenService.GetUserId();

            if (string.IsNullOrEmpty(model.AdminName))
                model.AdminName = "Admin";

            var updateDTO = _mapper.Map<UpdateCategoryDTO>(model);

            var response = await PutAsync<ServiceResponseDTO<ReadCategoryDTO>>(
                "api/category/update",
                updateDTO
            );

            return _mapper.Map<ReadCategoryResult>(response.Data);
        }



        public ReadCategoryResult GetCreateModel() => new();

        public async Task<ReadCategoryResult> CreateCategory(ReadCategoryResult model)
        {
            var dto = _mapper.Map<CreateCategoryDTO>(model);

            var response = await PostAsync<ServiceResponseDTO<ReadCategoryDTO>>(
                "api/category/create",
                JsonContent.Create(dto)
            );

            return _mapper.Map<ReadCategoryResult>(response.Data);
        }
    }
}
