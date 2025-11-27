using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CertificateTemplate;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Instructor")]
    public class CertificateTemplatesController : ControllerBase
    {
        private readonly ICertificateTemplateServices _templateService;

        public CertificateTemplatesController(ICertificateTemplateServices templateService)
        {
            _templateService = templateService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponseDTO<ReadCertificateTemplateDTO>>> Create(CreateCertificateTemplateDTO dto)
        {
            var result = await _templateService.CreateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ServiceResponseDTO<ReadCertificateTemplateDTO>>> Update(UpdateCertificateTemplateDTO dto)
        {
            var result = await _templateService.UpdateAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCertificateTemplateDTO>>> GetById(string id)
        {
            var result = await _templateService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponseDTO<IEnumerable<ReadCertificateTemplateDTO>>>> GetAll()
        {
            var result = await _templateService.GetAllAsync();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponseDTO<ReadCertificateTemplateDTO>>> Delete(string id)
        {
            var result = await _templateService.DeleteAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
