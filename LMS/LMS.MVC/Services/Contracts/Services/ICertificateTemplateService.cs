using LMS.MVC.Models.ViewModels.CertificateTemplate;
using LMS.MVC.Services.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface ICertificateTemplateService
    {
        Task<SuccessServiceResult<IEnumerable<CertificateTemplateViewModel>>> GetAllAsync();
        Task<SuccessServiceResult<CertificateTemplateViewModel>> GetByIdAsync(string id);
        Task<SuccessServiceResult<CertificateTemplateViewModel>> CreateAsync(CreateCertificateTemplateViewModel model);
        Task<SuccessServiceResult<CertificateTemplateViewModel>> UpdateAsync(UpdateCertificateTemplateViewModel model);
        Task<bool> DeleteAsync(string id);
    }
}
