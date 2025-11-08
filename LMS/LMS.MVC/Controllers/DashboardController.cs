using Domain.Enums;
using LMS.MVC.Models.ViewModels.Enrollment;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfServices _services;
        public DashboardController(IUnitOfServices services)
        {
            _services = services;
        }
        private ICategoryService CategoryService => _services.CategoryService;
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                var users = await _services.UserService.GetAllUsers();
                return View("Admin/UserManagement", users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View("Admin/UserManagement", new List<object>());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CategoryManagement()
        {
            try
            {
                var categories = await CategoryService.GetAllCategories();
                return View("Admin/CategoryManagement", categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View("Admin/CategoryManagement", new List<object>());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CourseEnrollments()
        {
            try
            {
                var vm = new EnrollmentManagementVM();

                vm.Request.StatusList = Enum.GetNames(typeof(ApplicationStatus))
                    .Select(s => new MVCStatusOptions
                    {
                        Value = s,
                        DisplayName = s
                    })
                    .ToList();
                var response = await _services.EnrollmentService
                    .GetEnrollmentsAsync(vm.Request);
                vm.Enrollments = response.Data.Select(e => new EnrollmentItemVM
                {
                    UserId = e.UserId,
                    CourseId = e.CourseId,
                    UserEmail = e.UserEmail,
                    UserName = e.UserName,
                    CourseName = e.CourseName,
                    CourseCode = e.CourseCode,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt
                }).ToList();

                vm.Request.StatusList = Enum.GetNames(typeof(ApplicationStatus))
                    .Select(s => new MVCStatusOptions
                    {
                        Value = s,
                        DisplayName = s
                    })
                    .ToList();
                return View("Admin/CourseEnrollments",vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading enrollment management page: {ex.Message}";
                return View(new EnrollmentManagementVM());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEnrollments(EnrollmentManagementVM vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Invalid input. Please check your filters.";
                    return View(vm);
                }

                var response = await _services.EnrollmentService
                    .GetEnrollmentsAsync(vm.Request);

                if (!response.Success)
                {
                    TempData["Error"] = response.Message;
                    return View(vm);
                }

                vm.Enrollments = response.Data.Select(e => new EnrollmentItemVM
                {
                    UserId = e.UserId,
                    CourseId = e.CourseId,
                    UserEmail = e.UserEmail,
                    UserName = e.UserName,
                    CourseName = e.CourseName,
                    CourseCode = e.CourseCode,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt
                }).ToList();

                vm.Request.StatusList = Enum.GetNames(typeof(ApplicationStatus))
                    .Select(s => new MVCStatusOptions
                    {
                        Value = s,
                        DisplayName = s
                    })
                    .ToList();

                return View("Admin/CourseEnrollments",vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fetching enrollments: {ex.Message}";
                return View("Admin/CourseEnrollments",new EnrollmentManagementVM());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveEnrollment(ApproveStudentEnrollment model)
        {
            try
            {
              

                var result = await _services.EnrollmentService.ApproveEnrollmentAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = "Enrollment approved successfully!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error approving enrollment: {ex.Message}";
            }

            return RedirectToAction("CourseEnrollments");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectEnrollment(ApproveStudentEnrollment model)
        {
            try
            {
                
                var result = await _services.EnrollmentService.DenyEnrollmentAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = "Enrollment rejected successfully!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error rejecting enrollment: {ex.Message}";
            }

            return RedirectToAction("CourseEnrollments");
        }



    }
}