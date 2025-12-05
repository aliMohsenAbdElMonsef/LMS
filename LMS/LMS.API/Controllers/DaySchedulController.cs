using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.DaySchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    public class DaySchedulController : ControllerBase
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class DaySchedulesController : ControllerBase
        {
            private readonly IUnitOfServices _unitOfServices;

            public DaySchedulesController(IUnitOfServices unitOfServices)
            {
                _unitOfServices = unitOfServices;
            }


            [HttpGet("course/{courseId}")]
            [AllowAnonymous]
            public async Task<IActionResult> GetCourseSchedules(string courseId)
            {
                try
                {
                    var result = await _unitOfServices.DaySchedules.GetCourseSchedulesAsync(courseId);
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


            [HttpGet("{scheduleId}")]
            [AllowAnonymous]
            public async Task<IActionResult> GetDaySchedule(string scheduleId)
            {
                try
                {
                    var result = await _unitOfServices.DaySchedules.GetByIdAsync(scheduleId);
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
            public async Task<IActionResult> CreateDaySchedule([FromBody] CreateDayScheduleDTO dto)
            {
                try
                {
                    if (!ModelState.IsValid)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid data"
                        });

                    var result = await _unitOfServices.DaySchedules.CreateAsync(dto);
                    return Ok(new
                    {
                        success = true,
                        data = result,
                        message = "Day schedule created successfully"
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


            [HttpPut("{scheduleId}")]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> UpdateDaySchedule(string scheduleId, [FromBody] UpdateDayScheduleDTO dto)
            {
                try
                {
                    if (scheduleId != dto.Id)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Schedule ID mismatch"
                        });

                    if (!ModelState.IsValid)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Invalid data"
                        });

                    var result = await _unitOfServices.DaySchedules.UpdateAsync(dto);
                    return Ok(new
                    {
                        success = true,
                        data = result,
                        message = "Day schedule updated successfully"
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


            [HttpDelete("{scheduleId}")]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> DeleteDaySchedule(string scheduleId)
            {
                try
                {
                    await _unitOfServices.DaySchedules.DeleteAsync(scheduleId);
                    return Ok(new
                    {
                        success = true,
                        message = "Day schedule deleted successfully"
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
