using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Lecture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    public class LectureController : ControllerBase
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class LecturesController : ControllerBase
        {
            private readonly IUnitOfServices _unitOfServices;

            public LecturesController(IUnitOfServices unitOfServices)
            {
                _unitOfServices = unitOfServices;
            }


            [HttpGet]
            [AllowAnonymous]
            public async Task<IActionResult> GetAllLectures()
            {
                try
                {
                    var result = await _unitOfServices.Lectures.GetAllAsync();
                    return Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }


            [HttpGet("{lectureId}")]
            [AllowAnonymous]
            public async Task<IActionResult> GetLecture(string lectureId)
            {
                try
                {
                    var result = await _unitOfServices.Lectures.GetByIdAsync(lectureId);
                    return Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }

            [HttpGet("course/{courseId}")]
            [AllowAnonymous]
            public async Task<IActionResult> GetCourseLectures(string courseId)
            {
                try
                {
                    var result = await _unitOfServices.Lectures.GetCourseOcturesAsync(courseId);
                    return Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }

            [HttpGet("instructor/{instructorId}")]
            [Authorize(Roles = "Instructor,Admin")]
            public async Task<IActionResult> GetInstructorLectures(string instructorId)
            {
                try
                {
                    var result = await _unitOfServices.Lectures.GetInstructorOcturesAsync(instructorId);
                    return Ok(new
                    {
                        success = true,
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }


            [HttpPost]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> CreateLecture([FromBody] CreateLectureDTO dto)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid data"
                        });

                    var result = await _unitOfServices.Lectures.CreateAsync(dto);
                    return Ok(new
                    {
                        success = true,
                        data = result,
                        message = "Lecture created successfully"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }

            [HttpPut("{lectureId}")]
            [Authorize(Roles = "Admin,Instructor")]
            public async Task<IActionResult> UpdateLecture(string lectureId, [FromBody] UpdateLectureDTO dto)
            {
                try
                {
                    if (lectureId != dto.Id)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Lecture ID mismatch"
                        });

                    if (!ModelState.IsValid)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid data"
                        });

                    var result = await _unitOfServices.Lectures.UpdateAsync(dto);
                    return Ok(new
                    {
                        success = true,
                        data = result,
                        message = "Lecture updated successfully"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }


            [HttpDelete("{lectureId}")]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> DeleteLecture(string lectureId)
            {
                try
                {
                    await _unitOfServices.Lectures.DeleteAsync(lectureId);
                    return Ok(new
                    {
                        success = true,
                        message = "lectures deleted successfully"
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }
        }
    }

}
