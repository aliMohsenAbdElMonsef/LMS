using LMS.BusinessLogic.DTOs.Certificate;
using LMS.BusinessLogic.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICertificateGenerationService
    {
        Task<ServiceResponseDTO<ReadStudentCertificateDTO>> GenerateCertificateAsync(GenerateCertificateDTO dto);
        Task<ServiceResponseDTO<IEnumerable<ReadStudentCertificateDTO>>> GetStudentCertificatesAsync(string studentId);
        Task<ServiceResponseDTO<byte[]>> DownloadCertificateAsync(string certificateId);
        Task<ServiceResponseDTO<ReadStudentCertificateDTO>> GetCertificateByIdAsync(string certificateId);
    }
}
