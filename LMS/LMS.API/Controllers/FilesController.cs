using LMS.BusinessLogic.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("course-thumbnails/{fileName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseThumbnail(string fileName)
        {
            try
            {
                var fileResult = await _fileService.GetCourseThumbnailAsync(fileName);

                if (fileResult == null)
                {
                    var defaultResult = await _fileService.GetDefaultThumbnailAsync();
                    if (defaultResult != null)
                        return defaultResult;

                    return NotFound(new { success = false, message = "Thumbnail not found" });
                }
                Response.Headers.Append("Cache-Control", "public, max-age=86400");
                return fileResult;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving thumbnail" });
            }
        }

        [HttpPost("course-thumbnails")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCourseThumbnail(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file uploaded" });

                var result = await _fileService.SaveCourseThumbnailAsync(file);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new
                {
                    success = true,
                    fileName = result.FileName,
                    message = "Thumbnail uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("course-thumbnails/{fileName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourseThumbnail(string fileName)
        {
            try
            {
                var result = await _fileService.DeleteCourseThumbnailAsync(fileName);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new { success = true, message = "Thumbnail deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}