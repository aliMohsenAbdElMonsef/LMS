using Domain.Enums;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Models.ViewModels.Enrollment;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index(string searchTerm, string category)
        {
            try
            {
                var courses = await _services.CourseService.GetAllCoursesAsync();
                if (!courses.Success)
                {
                    return View(Enumerable.Empty<ReadCourseResult>());
                }

                var filteredCourses = courses.Data.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filteredCourses = filteredCourses.Where(c =>
                        c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (c.Description != null && c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply category filter
                if (!string.IsNullOrWhiteSpace(category))
                {
                    filteredCourses = filteredCourses.Where(c => c.CategoryName == category);
                }

                ViewBag.SearchTerm = searchTerm;
                ViewBag.SelectedCategory = category;
                ViewBag.Categories = courses.Data.Select(c => c.CategoryName).Distinct().OrderBy(c => c).ToList();

                return View(filteredCourses);
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

                TempData["Success"] = "? Course created successfully!";
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

            TempData["Success"] = "? Course updated successfully!";
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString();
            var userRole = User.IsInRole("Instructor") ? "Instructor" :
                           User.IsInRole("Student") ? "Student" : "None";

            var enrollmentStatus = "None";
            if (userRole != "None" && !string.IsNullOrEmpty(userId))
            {
                enrollmentStatus = await _services.CourseService.IsUserEnrollIntoCourse(userId, id);
            }

            ViewBag.EnrollmentStatus = enrollmentStatus;
            ViewBag.IsEnrolled = enrollmentStatus == "Approved";

            return View(result.Data);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) 
                return Json(new { success = false, message = "Invalid course ID" });
            var success = await _services.CourseService.DeleteCourse(Guid.Parse(id));
            
            return Json(new { 
                success = success, 
                message = success ? "Course deleted successfully!" : "Failed to delete course." 
            });
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<IActionResult> Enroll(string CourseId)
        {

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated." });

                RequestErollmentintCourseViewModel model = new RequestErollmentintCourseViewModel
                {
                    UserId = userId,
                    CourseId = CourseId
                };
                var result = await _services.EnrollmentService.EnrollAsync(model);

                if (result.Success)
                {
                    bool isPending = User.IsInRole("Instructor");
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        canEnrollImmediately = !isPending
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<IActionResult> Unenroll(string CourseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated." });
                RequestErollmentintCourseViewModel model = new RequestErollmentintCourseViewModel
                {
                    UserId = userId,
                    CourseId = CourseId
                };

                var result = await _services.EnrollmentService.UnenrollAsync(model);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        unenrolled = true,
                        message = result.Message
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Student,Instructor")]
        public async Task<IActionResult> MyCourses()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Get all courses first
                var allCoursesResult = await _services.CourseService.GetAllCoursesAsync();
                if (!allCoursesResult.Success)
                {
                    TempData["Error"] = "Failed to load courses.";
                    return View(new List<ReadCourseResult>());
                }

                // Filter courses where user is enrolled
                var myCourses = new List<ReadCourseResult>();

                foreach (var course in allCoursesResult.Data)
                {
                    var enrollmentStatus = await _services.CourseService.IsUserEnrollIntoCourse(userId, course.Id);
                    if (enrollmentStatus == "Approved")
                    {
                        myCourses.Add(course);
                    }
                }

                ViewBag.UserRole = User.IsInRole("Instructor") ? "Instructor" : "Student";

                return View(myCourses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading your courses: {ex.Message}";
                return View(new List<ReadCourseResult>());
            }
        }
    }
}