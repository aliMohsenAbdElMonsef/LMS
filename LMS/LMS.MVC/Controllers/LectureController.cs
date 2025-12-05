using LMS.MVC.Models.ViewModels.Lecture;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class LectureController : Controller
    {
        private readonly IUnitOfServices _services;

        public LectureController(IUnitOfServices services)
        {
            _services = services;
        }

        // GET: Lecture/Index
        [HttpGet]
        public async Task<IActionResult> Index(string courseId)
        {
            try
            {
                var result = await _services.LectureService.GetLecturesByCourseAsync(courseId);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index", "Course");
                }

                ViewBag.CourseId = courseId;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Course");
            }
            }


        [HttpGet]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<IActionResult> MyLectures()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                var result = await _services.LectureService.GetMyLecturesAsync(userId, userRole);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index", "Home");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Watch(string id)
        {
            try
            {
                var result = await _services.LectureService.GetLectureByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Lecture not found.";
                    return RedirectToAction("Index");
                }
                
                // Track lecture view
                await _services.LectureService.TrackProgressAsync(id, 0); // Assuming 0 for just opening, or implement logic
                
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading lecture: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Lecture/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string courseId)
        {
            ViewBag.CourseId = courseId;
            
            // Fetch instructors for the dropdown
            if (Guid.TryParse(courseId, out var courseGuid))
            {
                var courseResult = await _services.CourseService.GetCourseDetails(courseGuid);
                if (courseResult.Success && courseResult.Data != null)
                {
                     ViewBag.Instructors = courseResult.Data.Instructors;
                }
            }

            return View(new CreateLectureViewModel { CourseId = courseId, LectureDate = DateTime.UtcNow.Date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateLectureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Guid.TryParse(model.CourseId, out var courseGuid))
                {
                    var courseResult = await _services.CourseService.GetCourseDetails(courseGuid);
                    if (courseResult.Success && courseResult.Data != null)
                    {
                         ViewBag.Instructors = courseResult.Data.Instructors;
                    }
                }
                return View(model);
            }

            try
            {
                var result = await _services.LectureService.CreateLectureAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Lecture created successfully!";
                    return RedirectToAction("Index", new { courseId = model.CourseId });
                }
                
                TempData["Error"] = result.Message;
                if (Guid.TryParse(model.CourseId, out var courseGuid))
                {
                    var courseResult = await _services.CourseService.GetCourseDetails(courseGuid);
                    if (courseResult.Success && courseResult.Data != null)
                    {
                         ViewBag.Instructors = courseResult.Data.Instructors;
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating lecture: {ex.Message}";
                if (Guid.TryParse(model.CourseId, out var courseGuid))
                {
                    var courseResult = await _services.CourseService.GetCourseDetails(courseGuid);
                    if (courseResult.Success && courseResult.Data != null)
                    {
                         ViewBag.Instructors = courseResult.Data.Instructors;
                    }
                }
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var result = await _services.LectureService.GetLectureByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Lecture not found.";
                    return RedirectToAction("Index");
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading lecture: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Lecture/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id, UpdateLectureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _services.LectureService.UpdateLectureAsync(id, model);
                if (result.Success)
                {
                    TempData["Success"] = "Lecture updated successfully!";
                    return RedirectToAction("Watch", new { id });
                }
                
                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating lecture: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id, string courseId)
        {
            try
            {
                var result = await _services.LectureService.DeleteLectureAsync(id);
                if (result)
                {
                    TempData["Success"] = "Lecture deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete lecture.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting lecture: {ex.Message}";
            }
            
            return RedirectToAction("Index", new { courseId });
        }

        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Launch(string id)
        {
            var result = await _services.LectureService.GetLectureByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            var model = new LaunchLectureViewModel
            {
                LectureId = result.Data.Id,
                CourseId = result.Data.CourseId,
                CourseName = result.Data.CourseName,
                LectureTitle = result.Data.Title,
                LectureDate = result.Data.LectureDate,
                StartTime = result.Data.StartTime,
                ZoomLink = result.Data.ZoomLink
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Launch(string id, string zoomLink)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(zoomLink))
            {
                TempData["Error"] = "Lecture ID and Zoom Link are required.";
                return RedirectToAction("MyLectures");
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var result = await _services.LectureService.LaunchLectureAsync(id, zoomLink);
            
            if (result.Success)
            {
                TempData["Success"] = "Lecture launched successfully. Students have been notified.";
                return RedirectToAction("MyLectures");
            }

            TempData["Error"] = result.Message;
            return RedirectToAction("MyLectures");
        }
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAttendanceStats()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found" });
            }

            var result = await _services.LectureService.GetAttendanceStatisticsAsync(userId);

            return Json(new
            {
                success = result.Success,
                data = result.Data,
                message = result.Message
            });
        }

        [Authorize(Roles = "Student,Instructor,Admin")]
        public async Task<IActionResult> Join(string id)
        {
            var result = await _services.LectureService.GetLectureByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(result.Data.ZoomLink))
            {
                TempData["Error"] = "This lecture has not been launched yet.";
                return RedirectToAction("Details", "Course", new { id = result.Data.CourseId });
            }

            if (User.IsInRole("Student"))
            {
                var lectureDateTime = result.Data.LectureDate.Date + result.Data.StartTime;
                var now = DateTime.Now;
                if (now < lectureDateTime.AddMinutes(-15) || now > lectureDateTime.AddHours(2))
                {
                    TempData["Error"] = "You can only join the lecture 15 minutes before it starts.";
                    return RedirectToAction("MyLectures");
                }
                
                // Mark student as attended
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var joinResult = await _services.LectureService.JoinLectureAsync(id);
                        if (!joinResult.Success)
                        {
                            TempData["Warning"] = $"You joined the lecture but attendance was not recorded: {joinResult.Message}";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Warning"] = $"You joined the lecture but attendance recording failed: {ex.Message}";
                    }
                }
            }

            // Redirect to Zoom link
            return Redirect(result.Data.ZoomLink);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reschedule(string id)
        {
            var result = await _services.LectureService.GetLectureByIdAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Lecture not found.";
                return RedirectToAction("Index", "Home");
            }

            var model = new RescheduleLectureViewModel
            {
                LectureId = result.Data.Id,
                CourseId = result.Data.CourseId,
                Title = result.Data.Title,
                CurrentDate = result.Data.LectureDate,
                CurrentStartTime = result.Data.StartTime,
                NewDate = result.Data.LectureDate,
                NewStartTime = result.Data.StartTime
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(RescheduleLectureViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _services.LectureService.RescheduleLectureAsync(model.LectureId, model.NewDate, model.NewStartTime);
            if (result.Success)
            {
                TempData["Success"] = "Lecture rescheduled successfully.";
                return RedirectToAction("Index", new { courseId = model.CourseId });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StreamRecording(string id)
        {
            try
            {
                var result = await _services.LectureService.GetLectureByIdAsync(id);
                if (!result.Success || result.Data == null || string.IsNullOrEmpty(result.Data.RecordingPath))
                {
                    return NotFound();
                }

                var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.API", "wwwroot");
                var filePath = Path.Combine(apiPath, "uploads", "lectures", "recordings", result.Data.RecordingPath);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var contentType = result.Data.RecordingPath.EndsWith(".mp4") ? "video/mp4" : 
                                 result.Data.RecordingPath.EndsWith(".mkv") ? "video/x-matroska" :
                                 result.Data.RecordingPath.EndsWith(".avi") ? "video/x-msvideo" :
                                 result.Data.RecordingPath.EndsWith(".mov") ? "video/quicktime" :
                                 "application/octet-stream";

                return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadMaterials(string id)
        {
            try
            {
                var result = await _services.LectureService.GetLectureByIdAsync(id);
                if (!result.Success || result.Data == null || string.IsNullOrEmpty(result.Data.MaterialsPath))
                {
                    TempData["Error"] = "Materials not found.";
                    return RedirectToAction("MyLectures");
                }

                // Files are stored in the API project's wwwroot folder
                var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.API", "wwwroot");
                var filePath = Path.Combine(apiPath, "uploads", "lectures", "materials", result.Data.MaterialsPath);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "Materials file not found.";
                    return RedirectToAction("MyLectures");
                }

                return PhysicalFile(filePath, "application/zip", result.Data.MaterialsPath);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading materials: {ex.Message}";
                return RedirectToAction("MyLectures");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UploadContent(string id)
        {
            var result = await _services.LectureService.GetLectureByIdAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Lecture not found.";
                return RedirectToAction("MyLectures");
            }

            var viewModel = new UploadLectureContentViewModel
            {
                LectureId = result.Data.Id,
                CourseId = result.Data.CourseId,
                LectureTitle = result.Data.Title,
                CurrentRecordingPath = result.Data.RecordingPath,
                CurrentMaterialsPath = result.Data.MaterialsPath
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UploadContent(UploadLectureContentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                var result = await _services.LectureService.UploadLectureContentAsync(model.LectureId, model.NewRecordingFile, model.NewMaterialsFile, userId, userRole);
                
                if (result.Success)
                {
                    TempData["Success"] = "Content uploaded successfully!";
                    return RedirectToAction("MyLectures");
                }

                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error uploading content: {ex.Message}";
                return View(model);
            }
        }

    }
}
