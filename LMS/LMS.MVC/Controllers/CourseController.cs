using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly LMS.MVC.Services.Contracts.IUnitOfServices _services;

        public CourseController(LMS.MVC.Services.Contracts.IUnitOfServices services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // This will call the API endpoint /api/course/all
                var courses = await _services.CourseService.GetAllCoursesAsync();
                return View(courses);
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = $"Error loading courses: {ex.Message}";
                return View(new List<LMS.BusinessLogic.DTOs.Course.ReadCourseDTO>());
            }
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var course = await _services.CourseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    TempData["Error"] = "Course not found";
                    return RedirectToAction("Index");
                }
                return View(course);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading course: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
