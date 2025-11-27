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

        // GET: Lecture/Watch/5
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
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Create(string courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: Lecture/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create(CreateLectureViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating lecture: {ex.Message}";
                return View(model);
            }
        }

        // GET: Lecture/Edit/5
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
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
        [Authorize(Roles = "Instructor,Admin")]
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
        [Authorize(Roles = "Instructor,Admin")]
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
    }
}
