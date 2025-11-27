using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Question;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public QuestionsController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuestionDTO>>> CreateQuestion(CreateQuestionDTO question)
        {
            var result = await _unitOfServices.Questions.CreateAsync(question);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuestionDTO>>> UpdateQuestion(UpdateQuestionDTO question)
        {
            var result = await _unitOfServices.Questions.UpdateAsync(question);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuestionDTO>>> DeleteQuestion(string id)
        {
            var result = await _unitOfServices.Questions.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<ReadQuestionDTO>>> GetQuestionById(string id)
        {
            var result = await _unitOfServices.Questions.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponseDTO<List<ReadQuestionDTO>>>> GetAllQuestions()
        {
            var result = await _unitOfServices.Questions.GetAllAsync();
            return Ok(result);
        }
    }
}
