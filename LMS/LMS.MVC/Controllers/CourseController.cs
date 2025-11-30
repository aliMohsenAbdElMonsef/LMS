using System;
using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Models.ViewModels.Enrollment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.MVC.Services.Contracts;
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

        [HttpGet]
        [AllowAnonymous]
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.IsInRole("Instructor") ? "Instructor"
                           : User.IsInRole("Student") ? "Student"
                           : "None";

            bool isEnrolled = false;

            if (userRole != "None" && !string.IsNullOrEmpty(userId))
            {
                isEnrolled = await _services.CourseService.IsUserEnrollIntoCourse(userId, id);
            }

            ViewBag.UserRole = userRole;
            ViewBag.IsEnrolled = isEnrolled;

            return View(result.Data);
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
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                var allCoursesResult = await _services.CourseService.GetAllCoursesAsync();
                if (!allCoursesResult.Success)
                {
                    TempData["Error"] = "Failed to load courses.";
                    return View(new List<ReadCourseResult>());
                }

                var myCourses = new List<ReadCourseResult>();

                foreach (var course in allCoursesResult.Data)
                {
                    bool isEnrolled = await _services.CourseService.IsUserEnrollIntoCourse(userId, course.Id);
                    bool isInstructor = course.Instructors.Any(i => i.Id == userId);
                    bool isAdmin = course.AdminId == userId;

                    if (isEnrolled || isInstructor || isAdmin)
                    {
                        myCourses.Add(course);
                    }
                }

                return View(myCourses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading your courses: {ex.Message}";
                return View(new List<ReadCourseResult>());
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel();
            
            // Populate categories
            var categories = await _services.CategoryService.GetAllCategories();
            if (categories != null)
            {
                model.AvailableCategories = categories.Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();
            }

            // Populate Delivery Modes
            ViewBag.DeliveryModes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(Enum.GetValues(typeof(Domain.Enums.DeliveryMode)));

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(model.AdminId) && !string.IsNullOrEmpty(userId))
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToList();
                }

                // Repopulate Delivery Modes
                ViewBag.DeliveryModes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(Enum.GetValues(typeof(Domain.Enums.DeliveryMode)));

                return View(model);
            }

            // Set the AdminId to the current user if not set (though it might be hidden in form)
            if (string.IsNullOrEmpty(model.AdminId) && !string.IsNullOrEmpty(userId))
            {
                model.AdminId = userId;
            }

            var result = await _services.CourseService.CreateCourse(model);

            if (result.Success)
            {
                TempData["Success"] = "Course created successfully!";
                // Redirect to the details of the newly created course if possible, or Index
                // Assuming result.Data contains the created course with its ID
                if (result.Data != null)
                {
                    return RedirectToAction(nameof(Details), new { id = result.Data.Id });
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Message ?? "Failed to create course.";

            // Repopulate on failure
            var catResult = await _services.CategoryService.GetAllCategories();
            if (catResult != null)
            {
                model.AvailableCategories = catResult.Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();
            }
            ViewBag.DeliveryModes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(Enum.GetValues(typeof(Domain.Enums.DeliveryMode)));

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var result = await _services.CourseService.GetCourseDetails(Guid.Parse(id));
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            var course = result.Data;
            var model = new EditCourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                CourseCode = course.CourseCode,
                Credits = course.Credits,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Level = course.Level,
                Language = course.Language,
                DeliveryMode = course.DeliveryMode,
                Status = course.Status,
                Price = course.Price,
                IsFree = course.IsFree,
                MinAttendancePercentage = course.MinAttendancePercentage,
                MinPerformanceScore = course.MinPerformanceScore,
                AutoIssueCertificates = course.AutoIssueCertificates,
                CertificateTemplateId = course.CertificateTemplateId,
                CategoryId = course.CategoryId,
                CategoryName = course.CategoryName,
                ThumbnailPath = course.ThumbnailPath,
                EveryStuCouldEnroll = course.EveryStuCouldEnroll,
                AdminId = course.AdminId
            };

            var categories = await _services.CategoryService.GetAllCategories();
            if (categories != null)
            {
                model.AvailableCategories = categories.Select(c => new CategoryOption 
                { 
                    Id = c.Id, 
                    Name = c.Name 
                }).ToList();

                // Ensure CategoryName is set if we have the CategoryId
                if (!string.IsNullOrEmpty(model.CategoryId))
                {
                    // Try to find the category by ID (case-insensitive comparison)
                    var category = model.AvailableCategories.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Id) && 
                        c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                    
                    if (category != null && !string.IsNullOrEmpty(category.Name))
                    {
                        model.CategoryName = category.Name;
                    }
                    else if (string.IsNullOrEmpty(model.CategoryName))
                    {
                        // If still not found, try direct lookup from the categories list
                        var directCategory = categories.FirstOrDefault(c => 
                            !string.IsNullOrEmpty(c.Id) && 
                            c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                        
                        if (directCategory != null && !string.IsNullOrEmpty(directCategory.Name))
                        {
                            model.CategoryName = directCategory.Name;
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload categories if validation fails
                var categories = await _services.CategoryService.GetAllCategories();
                model.AvailableCategories = categories?.Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList() ?? new List<CategoryOption>();

                // Ensure CategoryName is set if we have the CategoryId
                if (!string.IsNullOrEmpty(model.CategoryId) && model.AvailableCategories != null)
                {
                    var category = model.AvailableCategories.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Id) && 
                        c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                    
                    if (category != null && !string.IsNullOrEmpty(category.Name))
                    {
                        model.CategoryName = category.Name;
                    }
                    else if (categories != null)
                    {
                        var directCategory = categories.FirstOrDefault(c => 
                            !string.IsNullOrEmpty(c.Id) && 
                            c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                        
                        if (directCategory != null && !string.IsNullOrEmpty(directCategory.Name))
                        {
                            model.CategoryName = directCategory.Name;
                        }
                    }
                }

                return View(model);
            }

            // Check for duplicate course code
            var allCourses = await _services.CourseService.GetAllCoursesAsync();
            if (allCourses.Success && allCourses.Data.Any(c =>
                c.CourseCode.Equals(model.CourseCode, StringComparison.OrdinalIgnoreCase)
                && c.Id != model.Id))
            {
                TempData["Error"] = "Course code already exists!";
                
                // Reload categories and set CategoryName
                var categories = await _services.CategoryService.GetAllCategories();
                model.AvailableCategories = categories?.Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList() ?? new List<CategoryOption>();

                // Ensure CategoryName is set if we have the CategoryId
                if (!string.IsNullOrEmpty(model.CategoryId) && model.AvailableCategories != null)
                {
                    var category = model.AvailableCategories.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Id) && 
                        c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                    
                    if (category != null && !string.IsNullOrEmpty(category.Name))
                    {
                        model.CategoryName = category.Name;
                    }
                    else if (categories != null)
                    {
                        var directCategory = categories.FirstOrDefault(c => 
                            !string.IsNullOrEmpty(c.Id) && 
                            c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                        
                        if (directCategory != null && !string.IsNullOrEmpty(directCategory.Name))
                        {
                            model.CategoryName = directCategory.Name;
                        }
                    }
                }

                return View(model);
            }

            // Call the service to update
            var result = await _services.CourseService.UpdateCourse(Guid.Parse(model.Id), model);

            if (result.Success)
            {
                TempData["Success"] = "Course updated successfully!";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            TempData["Error"] = result.Message ?? "Failed to update course.";

            // Reload categories for view
            var catResult = await _services.CategoryService.GetAllCategories();
            model.AvailableCategories = catResult?.Select(c => new CategoryOption
            {
                Id = c.Id,
                Name = c.Name
            }).ToList() ?? new List<CategoryOption>();

            // Ensure CategoryName is set if we have the CategoryId
            if (!string.IsNullOrEmpty(model.CategoryId) && model.AvailableCategories != null)
            {
                var category = model.AvailableCategories.FirstOrDefault(c => 
                    !string.IsNullOrEmpty(c.Id) && 
                    c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                
                if (category != null && !string.IsNullOrEmpty(category.Name))
                {
                    model.CategoryName = category.Name;
                }
                else if (catResult != null)
                {
                    var directCategory = catResult.FirstOrDefault(c => 
                        !string.IsNullOrEmpty(c.Id) && 
                        c.Id.Equals(model.CategoryId, StringComparison.OrdinalIgnoreCase));
                    
                    if (directCategory != null && !string.IsNullOrEmpty(directCategory.Name))
                    {
                        model.CategoryName = directCategory.Name;
                    }
                }
            }

            return View(model);
        }

    }
}
