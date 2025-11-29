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
                    TempData["Error"] = $"Error loading lectures: {result.Message}";
                    return RedirectToAction("Index", "Course");
                }

                ViewBag.CourseId = courseId;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading lectures: {ex.Message}";
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
                    TempData["Error"] = $"Error loading lectures: {result.Message}";
                    return RedirectToAction("Index", "Home");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading lectures: {ex.Message}";
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

        // POST: Lecture/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateLectureViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate instructors on error
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
                // Re-populate instructors on error
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
                // Re-populate instructors on error
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

        // GET: Lecture/Edit/5
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

        // POST: Lecture/Delete/5
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
        public async Task<IActionResult> Launch(LaunchLectureViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var result = await _services.LectureService.LaunchLectureAsync(model.LectureId, model.ZoomLink);
            if (result.Success)
            {
                TempData["Success"] = "Lecture launched successfully. Students have been notified.";
                // Redirect to Course Details using the CourseId from the model
                return RedirectToAction("Details", "Course", new { id = model.CourseId });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
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

            // Time validation for students
            if (User.IsInRole("Student"))
            {
                var lectureDateTime = result.Data.LectureDate.Date + result.Data.StartTime;
                var now = DateTime.UtcNow;
                // Allow joining 15 minutes before start until 2 hours after start
                if (now < lectureDateTime.AddMinutes(-15) || now > lectureDateTime.AddHours(2))
                {
                    TempData["Error"] = "You can only join the lecture 15 minutes before it starts.";
                    return RedirectToAction("MyLectures");
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

    }
}
