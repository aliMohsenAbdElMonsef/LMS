using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsEnrollmentsController : ControllerBase
    {
        private readonly IUnitOfServices _unitOfServices;

        public StudentsEnrollmentsController(IUnitOfServices unitOfServices)
        {
            _unitOfServices = unitOfServices;

        }
        private IStudentEnrollIntoCourseServices StudentEnrollIntoCourseServices => _unitOfServices.StudentEnrollIntoCourse;


        #region Enrollment Management

        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent([FromBody] CreateStudentEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.EnrollStudentAsync(dto);
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

        [HttpPut("unenroll")]
        public async Task<IActionResult> UnenrollStudent(StudentUnenrollfromCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.UnenrollStudentFromCourseAsync(dto);
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

        [HttpGet("Get")]
        public async Task<IActionResult> GetEnrollmentById(GetEnrollmentDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetEnrollmentByIdAsync(dto);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentEnrollments(string studentId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetStudentEnrollmentsAsync(studentId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseEnrollments(string courseId)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetCourseEnrollmentsAsync(courseId);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Check & Validation Endpoints

        [HttpGet("check-enrollment")]
        public async Task<IActionResult> IsStudentEnrolled(GetEnrollmentDTO dto)
        {
            try
            {
                var isEnrolled = await StudentEnrollIntoCourseServices.IsStudentEnrolledAsync(dto);
                return Ok(new { IsEnrolled = isEnrolled });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/count")]
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

        [HttpGet("course/{courseId}/average-progress")]
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

        [HttpGet("course/{courseId}/low-progress")]
        public async Task<IActionResult> GetStudentsWithLowProgress(GetStudentLessThersholdDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetStudentsWithLowProgressAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        #endregion

        #region Base CRUD Operations (from IBaseService)

        [HttpGet("enrollments")]
        public async Task<IActionResult> GetAllEnrollments()
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.GetAllAsync();

                return Ok(new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = true,
                    Message = "Enrollments retrieved successfully.",
                    Data = result.Data.ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseDTO<List<ReadStudentEnrollIntoCourseDTO>>
                {
                    Success = false,
                    Message = "An internal server error occurred.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateEnrollment([FromBody] CreateStudentEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.CreateAsync(dto);

                if (result != null) 
                {
                    return Ok(new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = true,
                        Message = "Enrollment created successfully.",
                        Data = result
                    });
                }
                else
                {
                    return BadRequest(new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Failed to create enrollment.",
                        Errors = new List<string> { "Creation operation returned null." }
                    });
                }
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

        [HttpPut("update")]
        public async Task<IActionResult> UpdateEnrollment([FromBody] UpdateStudentEnrollIntoCourseDTO dto)
        {
            try
            {
                var result = await StudentEnrollIntoCourseServices.UpdateAsync(dto);

                if (result != null) 
                {
                    return Ok(new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = true,
                        Message = "Enrollment updated successfully.",
                        Data = result
                    });
                }
                else
                {
                    return BadRequest(new ServiceResponseDTO<ReadStudentEnrollIntoCourseDTO>
                    {
                        Success = false,
                        Message = "Failed to update enrollment.",
                        Errors = new List<string> { "Update operation returned null or enrollment not found." }
                    });
                }
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
    }
    }