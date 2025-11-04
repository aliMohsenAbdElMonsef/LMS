using Domain.Enums;
using LMS.BusinessLogic.DTOs.Course;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Views.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly IUnitOfServices _services;

        public CourseController(IUnitOfServices services)
        {
            _services = services;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var courses = await _services.CourseService.GetAllCoursesAsync(); 
                if (!courses.Success)
                {
                    return View(Enumerable.Empty<ReadCourseResult>());
                }

                return View(courses.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading courses: {ex.Message}";
                return View(new List<ReadCourseResult>());
            }
        }

        private async Task LoadDropdownData(CreateCourseViewModel model)
        {
            var categories = await _services.CategoryService.GetAllCategories();

            model.AvailableCategories = categories?.Select(c => new Models.ViewModels.Course.CategoryOption
            {
                Id = c.Id,
                Name = c.Name
            }).ToList() ?? new List<Models.ViewModels.Course.CategoryOption>();

            ViewBag.DeliveryModes = Enum.GetValues(typeof(DeliveryMode))
                .Cast<DeliveryMode>()
                .Select(m => new SelectListItem
                {
                    Value = ((int)m).ToString(),
                    Text = m.ToString()
                }).ToList();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                StartDate = DateTime.Today.AddDays(7),
                EndDate = DateTime.Today.AddDays(91),
                AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            };

            await LoadDropdownData(model);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model);
                return View(model);
            }

            try
            {
                var result = await _services.CourseService.CreateCourse(model);
                

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    await LoadDropdownData(model);
                    return View(model);
                }

                TempData["Success"] = "✅ Course created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                await LoadDropdownData(model);
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var createModel = new CreateCourseViewModel
            {
                AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            };

            await LoadDropdownData(createModel);

            var result = await _services.CourseService.GetCourseForEdit(Guid.Parse(id));
          
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Course not found!";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Data);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData(model);
                return View(model);
            }

            var result = await _services.CourseService.UpdateCourse(Guid.Parse(model.Id), model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                await LoadDropdownData(model);
                return View(model);
            }

            TempData["Success"] = "✅ Course updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();
            var result = await _services.CourseService.GetCourseDetails(Guid.Parse(id));
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Course not found!";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var success = await _services.CourseService.DeleteCourse(Guid.Parse(id));
            if (success)
                TempData["Success"] = "✅ Course deleted successfully!";
            else
                TempData["Error"] = "Failed to delete course.";

            return RedirectToAction(nameof(Index));
        }
    }
}
