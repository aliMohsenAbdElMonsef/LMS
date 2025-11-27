using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Response;
using LMS.MVC.Models.ViewModels.CertificateTemplate;
using System.Net.Http.Json;

namespace LMS.MVC.Services.Services
{
    internal class CertificateTemplateService : BaseMVCServices, ICertificateTemplateService
    {
        public CertificateTemplateService(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task<SuccessServiceResult<IEnumerable<CertificateTemplateViewModel>>> GetAllAsync()
        {
            try
            {
                var response = await GetAsync<ApiResponse<IEnumerable<CertificateTemplateViewModel>>>("api/certificatetemplates");
                
                if (response?.Success == true)
                {
                    return new SuccessServiceResult<IEnumerable<CertificateTemplateViewModel>>
                    {
                        Success = true,
                        Data = response.Data ?? Enumerable.Empty<CertificateTemplateViewModel>()
                    };
                }
                
                return new SuccessServiceResult<IEnumerable<CertificateTemplateViewModel>>
                {
                    Success = false,
                    Message = response?.Message ?? "Failed to retrieve templates",
                    Data = Enumerable.Empty<CertificateTemplateViewModel>()
                };
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<IEnumerable<CertificateTemplateViewModel>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<SuccessServiceResult<CertificateTemplateViewModel>> GetByIdAsync(string id)
        {
            try
            {
                var response = await GetAsync<ApiResponse<CertificateTemplateViewModel>>($"api/certificatetemplates/{id}");
                
                if (response?.Success == true)
                {
                    return new SuccessServiceResult<CertificateTemplateViewModel> { Success = true, Data = response.Data };
                }
                
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = response?.Message ?? "Template not found" };
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<SuccessServiceResult<CertificateTemplateViewModel>> CreateAsync(CreateCertificateTemplateViewModel model)
        {
            try
            {
                var response = await PostAsync<ApiResponse<CertificateTemplateViewModel>>("api/certificatetemplates", JsonContent.Create(model));
                
                if (response?.Success == true)
                {
                    return new SuccessServiceResult<CertificateTemplateViewModel> { Success = true, Data = response.Data };
                }
                
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = response?.Message ?? "Failed to create template" };
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<SuccessServiceResult<CertificateTemplateViewModel>> UpdateAsync(UpdateCertificateTemplateViewModel model)
        {
            try
            {
                var response = await PutAsync<ApiResponse<CertificateTemplateViewModel>>("api/certificatetemplates", JsonContent.Create(model));
                
                if (response?.Success == true)
                {
                    return new SuccessServiceResult<CertificateTemplateViewModel> { Success = true, Data = response.Data };
                }
                
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = response?.Message ?? "Failed to update template" };
            }
            catch (Exception ex)
            {
                return new SuccessServiceResult<CertificateTemplateViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var response = await DeleteAsync<ApiResponse<object>>($"api/certificatetemplates/{id}");
                return response?.Success == true;
            }
            catch
            {
                return false;
            }
        }

        // Helper class to deserialize API responses
        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}
