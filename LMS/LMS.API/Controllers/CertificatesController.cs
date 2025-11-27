using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Certificate;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateGenerationService _certificateService;

        public CertificatesController(ICertificateGenerationService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<ServiceResponseDTO<ReadStudentCertificateDTO>>> GenerateCertificate(GenerateCertificateDTO dto)
        {
            var result = await _certificateService.GenerateCertificateAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-certificates")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> GetMyCertificates()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _certificateService.GetStudentCertificatesAsync(studentId);
            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> GetStudentCertificates(string studentId)
        {
            var result = await _certificateService.GetStudentCertificatesAsync(studentId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponseDTO<ReadStudentCertificateDTO>>> GetCertificateById(string id)
        {
            var result = await _certificateService.GetCertificateByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("download/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadCertificate(string id)
        {
            var result = await _certificateService.DownloadCertificateAsync(id);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return File(result.Data, "application/pdf", $"certificate_{id}.pdf");
        }
    }
}
