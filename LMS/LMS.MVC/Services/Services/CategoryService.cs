using AutoMapper;
using Azure.Core;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Category;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts.Services;
using System.Net.Http.Headers;

namespace LMS.MVC.Services.Services
{
    internal class CategoryService : BaseMVCServices, ICategoryService
    {
        private readonly ITokenService _tokenService;

        private readonly IMapper _mapper;
        public CategoryService(HttpClient client, ITokenService tokenService, IMapper mapper): base(client) 
        { 
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReadCategoryResult>> GetAllCategories()
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await _client.GetAsync("api/category/all");

            if (response.IsSuccessStatusCode) 
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<IEnumerable<ReadCategoryDTO>>>();

                var dtoCategories = serviceResponse?.Data ?? new List<ReadCategoryDTO>();

                var result = dtoCategories.Select(dto => new ReadCategoryResult
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    CreationDate = dto.CreationDate,
                    LastUpdated = dto.LastUpdated,
                    AdminId = dto.AdminId,
                    AdminName = dto.AdminName,
                    CoursesCount = dto.CoursesCount
                });
                return result;
                
            }
            return new List<ReadCategoryResult>();

        }

        public async Task<CategoryDetailsResult> GetCategoryById(string Id)
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
            var response = await _client.GetAsync($"api/category/details/{Id}");
            if (response.IsSuccessStatusCode)
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<CategoryDetailsDTO>>();

                var category = serviceResponse?.Data ?? new CategoryDetailsDTO();
                var result = new CategoryDetailsResult
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreationDate = category.CreationDate,
                    LastUpdated = category.LastUpdated,
                    CoursesCount = category.CoursesCount,
                    Courses = _mapper.Map<List<ReadCourseResult>>(category.Courses)
                };
                return result;
            }

            return new CategoryDetailsResult();

        }

        public async Task<ReadCategoryResult> GetEditModel(string Id)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.GetAsync($"api/category/{Id}");
            if (response.IsSuccessStatusCode)
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<ReadCategoryDTO>>();

                var category = serviceResponse?.Data ?? new ReadCategoryDTO();
                var result = new ReadCategoryResult
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreationDate = category.CreationDate,
                    LastUpdated = category.LastUpdated,
                    CoursesCount = category.CoursesCount,
                    AdminId = category.AdminId,
                    AdminName = category.AdminName,
                };
                return result;
            }

            return new ReadCategoryResult();

        }
        public async Task<ReadCategoryResult> EditCategory(ReadCategoryResult model)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var updateDto = new UpdateCategoryDTO
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
            };
            var response = await _client.PutAsJsonAsync("api/category/update", updateDto);

            if (response.IsSuccessStatusCode)
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<ReadCategoryDTO>>();

                var category = serviceResponse?.Data ?? new ReadCategoryDTO();
                var result = new ReadCategoryResult
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreationDate = category.CreationDate,
                    LastUpdated = category.LastUpdated,
                    CoursesCount = category.CoursesCount,
                    AdminName = category.AdminName,
                    AdminId = category.AdminId
                };
                return result;
            }

            return new ReadCategoryResult();

        }

        public ReadCategoryResult GetCreateModel()
        {
            return new ReadCategoryResult();
        }

        public async Task<ReadCategoryResult> CreateCategory(ReadCategoryResult model)
        {
            var token = await _tokenService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            CreateCategoryDTO dto = new CreateCategoryDTO { 
                Name = model.Name,
                Description = model.Description,
            };
            var response = await _client.PostAsJsonAsync("api/category/create", dto);
            if (response.IsSuccessStatusCode)
            {
                var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponseDTO<ReadCategoryDTO>>();

                var category = serviceResponse?.Data ?? new ReadCategoryDTO();
                var result = new ReadCategoryResult
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    CreationDate = category.CreationDate,
                    LastUpdated = category.LastUpdated,
                    CoursesCount = category.CoursesCount,
                    AdminName = category.AdminName,
                    AdminId = category.AdminId
                };
                return result;
            }

            return new ReadCategoryResult();
        }
    }
}
