using LMS.MVC.Models.ViewModels.Enrollment;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly IUnitOfServices _services;

        public EnrollmentController(IUnitOfServices services)
        {
            _services = services;
        }

        // GET: Enrollment/MyEnrollments
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyEnrollments()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var model = new EnrollmentManagementRequest();
                var enrollments = await _services.EnrollmentService.GetEnrollmentsAsync(model);
                return View(enrollments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading enrollments: {ex.Message}";
                return View(new List<object>());
            }
        }

        // POST: Enrollment/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Enroll(string courseId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var model = new RequestErollmentintCourseViewModel { UserId = userId, CourseId = courseId };
                var result = await _services.EnrollmentService.EnrollAsync(model);
                
                if (result.Success)
                {
                    TempData["Success"] = "Enrollment request submitted successfully!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
                
                return RedirectToAction("Details", "Course", new { id = courseId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error enrolling: {ex.Message}";
                return RedirectToAction("Index", "Course");
            }
        }

        // POST: Enrollment/Unenroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Unenroll(string enrollmentId)
        {
            try
            {
                var model = new RequestErollmentintCourseViewModel { CourseId = enrollmentId };
                var result = await _services.EnrollmentService.UnenrollAsync(model);
                
                if (result.Success)
                {
                    TempData["Success"] = "Unenrolled successfully!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
                
                return RedirectToAction("MyEnrollments");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unenrolling: {ex.Message}";
                return RedirectToAction("MyEnrollments");
            }
        }

        // GET: Enrollment/Pending
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Pending()
        {
            try
            {
                var model = new EnrollmentManagementRequest();
                var pendingEnrollments = await _services.EnrollmentService.GetEnrollmentsAsync(model);
                return View(pendingEnrollments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading pending enrollments: {ex.Message}";
                return View(new List<object>());
            }
        }

        // POST: Enrollment/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Approve(string userId, string courseId)
        {
            try
            {
                var model = new ApproveStudentEnrollment { UserId = userId, CourseId = courseId };
                var result = await _services.EnrollmentService.ApproveEnrollmentAsync(model);
                
                if (result.Success)
                {
                    TempData["Success"] = "Enrollment approved!";
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
            
            return RedirectToAction("Pending");
        }

        // POST: Enrollment/Deny
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Deny(string userId, string courseId)
        {
            try
            {
                var model = new ApproveStudentEnrollment { UserId = userId, CourseId = courseId };
                var result = await _services.EnrollmentService.DenyEnrollmentAsync(model);
                
                if (result.Success)
                {
                    TempData["Success"] = "Enrollment denied!";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error denying enrollment: {ex.Message}";
            }
            
            return RedirectToAction("Pending");
        }
    }
}
