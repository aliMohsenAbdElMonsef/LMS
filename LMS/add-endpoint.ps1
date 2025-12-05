$file = "LMS.API\Controllers\LectureController.cs"
$content = Get-Content $file -Raw
$newEndpoint = @"

        [HttpPost("{lectureId}/upload-content")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UploadLectureContent(string lectureId, [FromForm] IFormFile? recording, [FromForm] IFormFile? materials)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _unitOfServices.Lectures.UploadLectureContentAsync(lectureId, recording, materials, userId, userRole);
            return result.Success ? Ok(result) : BadRequest(result);
        }
"@

$searchPattern = "        }"
$replaceWith = "        }$newEndpoint"
$content = $content -replace "(\s+return result\.Success \? Ok\(result\) : BadRequest\(result\);\r\n\s+}\r\n)(\s+private ServiceResponseDTO<T> CreateErrorResponse)", "`$1$newEndpoint`r`n`$2"
$content | Set-Content $file -NoNewline
Write-Host "Endpoint added successfully!"
