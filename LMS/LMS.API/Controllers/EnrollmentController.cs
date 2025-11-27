using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public EnrollmentController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;

        }
        private string? GetCurrentUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        private IStudentEnrollment StudentEnrollIntoCourseServices => _unitOfServices.StudentEnrollIntoCourse;

        private IInstructorEnrollIntoCourse InstructorEnrollIntoCourse => _unitOfServices.InstructorEnrollIntoCourse;

        #region Enrollment Management

        [HttpPost("student/enroll")]
        [Authorize(Roles =("Student"))]
        public async Task<IActionResult> EnrollStudent([FromBody] RequestEnrollIntoCourseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            try
            {
                dto.UserId = GetCurrentUserId() ?? string.Empty;
                var result = await StudentEnrollIntoCourseServices.EnrollAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicResponseDTO
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
       

        [HttpPost("student/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveEnrollment([FromBody] UpdateStudentEnrollmentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            var result = await _unitOfServices.StudentEnrollIntoCourse.ApproveEnrollment(dto);
            return result.Success?Ok(result):BadRequest(result);
            
        }
        [HttpPost("student/deny")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DenyEnrollment([FromBody] UpdateStudentEnrollmentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            var result = await _unitOfServices.StudentEnrollIntoCourse.DenyEnrollment(dto);
            return result.Success ? Ok(result) : BadRequest(result);

        }

        [HttpPut("student/unenroll")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UnenrollStudent(RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.UnenrollFromCourseAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicResponseDTO
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        

        #region Retrieval Endpoints

        [HttpGet("student/get")]
        public async Task<IActionResult> GetEnrollmentById(RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetEnrollmentByIdAsync(dto);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<ReadEnrollIntoCourseDTO>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> GetStudentEnrollments(string studentId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetEnrollmentsAsync(studentId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("student/course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetCourseEnrollments(string courseId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetCourseEnrollmentsAsync(courseId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Check & Validation Endpoints

        [HttpGet("student/check-enrollment")]
        [Authorize(Roles ="Admin,Student")]
        public async Task<IActionResult> IsStudentEnrolled(RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var isEnrolled = await StudentEnrollIntoCourseServices.IsUserEnrolledAsync(dto);
                return Ok(new { IsEnrolled = isEnrolled });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("student/course/{courseId}/count")]
        public async Task<IActionResult> GetCourseEnrollmentCount(string courseId)
        {
            try
            {
                var count = await StudentEnrollIntoCourseServices.GetCourseEnrollmentCountAsync(courseId);
                return Ok(new { CourseId = courseId, EnrollmentCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        #endregion

        #region Analytics & Statistics Endpoints

        [HttpGet("student/course/{courseId}/average-progress")]
        public async Task<IActionResult> GetAverageProgressForCourse(string courseId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetAverageProgressForCourseAsync(courseId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<double>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("student/course/{courseId}/low-progress")]
        public async Task<IActionResult> GetStudentsWithLowProgress( string courseId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetStudentsWithLowProgress(courseId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion



        // instrcutor
        #region Enrollment Management

        [HttpPost("instructor/enroll")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> EnrollInstructor([FromBody] RequestEnrollIntoCourseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            try
            {
                dto.UserId = GetCurrentUserId() ?? string.Empty;
                var result = await InstructorEnrollIntoCourse.EnrollAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicResponseDTO
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("instructor/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveInstructorEnrollment([FromBody] UpdateEnrollIntoCourseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var result = await InstructorEnrollIntoCourse.ApproveEnrollment(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("instructor/deny")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DenyInstructorEnrollment([FromBody] UpdateEnrollIntoCourseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            var result = await InstructorEnrollIntoCourse.DenyEnrollment(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("instructor/unenroll")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UnenrollInstructor([FromBody] RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await InstructorEnrollIntoCourse.UnenrollFromCourseAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BasicResponseDTO
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Retrieval Endpoints

        [HttpGet("instructor/get")]
        public async Task<IActionResult> GetInstructorEnrollmentById([FromQuery] RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await InstructorEnrollIntoCourse.GetEnrollmentByIdAsync(dto);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<ReadEnrollIntoCourseDTO>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("instructor/{instructorId}")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> GetInstructorEnrollments(string instructorId)
        {
            try
            {
                var result = await InstructorEnrollIntoCourse.GetEnrollmentsAsync(instructorId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("instructor/course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetCourseInstructors(string courseId)
        {
            try
            {
                var result = await InstructorEnrollIntoCourse.GetCourseEnrollmentsAsync(courseId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Check & Validation Endpoints

        [HttpGet("instructor/check-enrollment")]
        [Authorize(Roles = "Admin, Instructor")]
        public async Task<IActionResult> IsInstructorEnrolled([FromQuery] RequestEnrollIntoCourseDTO dto)
        {
            try
            {
                var isEnrolled = await InstructorEnrollIntoCourse.IsUserEnrolledAsync(dto);
                return Ok(new { IsEnrolled = isEnrolled });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("instructor/course/{courseId}/count")]
        public async Task<IActionResult> GetCourseInstructorCount(string courseId)
        {
            try
            {
                var count = await InstructorEnrollIntoCourse.GetCourseEnrollmentCountAsync(courseId);
                return Ok(new { CourseId = courseId, InstructorCount = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        #endregion


        #region Base CRUD

        [HttpGet("all-filtered")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFilteredEnrollments([FromQuery] FilterEnrollmentDto dto)
        {
            try
            {
                var result = await _unitOfServices.EnrollmentManagement.GetFilteredEnrollmentsAsync(dto.Status,dto.UserSearch,dto.CourseCode,dto.Role);

                return Ok(new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = "Instructor enrollments retrieved successfully.",
                    Data = result.Data.ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion
    }
}