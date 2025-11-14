using LMS.MVC.Models.ViewModels.Assignment;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Entity.Enums;
namespace LMS.MVC.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IUnitOfServices _services;
        private readonly ITokenService _tokenService;

        public AssignmentController(IUnitOfServices services, ITokenService tokenService)
        {
            _services = services;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(string courseId)
        {
            var model = _services.AssignmentService.GetCreateModel();
            model.CourseId = courseId;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(ReadAssignmentResult model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Property: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View("Create", model);
            }

            try
            {
                var createdAssignment = await _services.AssignmentService.CreateAssignment(model);
                if (createdAssignment != null && !string.IsNullOrEmpty(createdAssignment.Id))
                {
                    TempData["SuccessMessage"] = "Assignment created successfully!";
                    return RedirectToAction("Details", new { id = createdAssignment.Id });
                }

                ModelState.AddModelError("", "Failed to create assignment. Please try again.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while creating assignment: {ex.Message}");
                return View("Create", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var assignment = await _services.AssignmentService.GetAssignmentById(id);
            if (assignment == null)
                return NotFound();
            // ADD THIS LINE - Set the current student ID
            ViewBag.CurrentStudentId = _tokenService.GetUserId();
            return View("Details", assignment);
        }


        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(string id)
        {
            var assignment = await _services.AssignmentService.GetEditModel(id);
            if (assignment == null) return NotFound();
            return View("Edit", assignment);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(ReadAssignmentResult model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Property: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View("Edit", model);
            }

            try
            {
                var updatedAssignment = await _services.AssignmentService.EditAssignment(model);
                if (updatedAssignment != null && !string.IsNullOrEmpty(updatedAssignment.Id))
                {
                    TempData["SuccessMessage"] = "Assignment updated successfully!";
                    return RedirectToAction("Details", new { id = updatedAssignment.Id });
                }

                ModelState.AddModelError("", "Failed to update assignment. Please try again.");
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while updating assignment: {ex.Message}");
                return View("Edit", model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(string id)
        {
            var studentId = _tokenService.GetUserId();
            var studentAssignment = await _services.AssignmentService.GetStudentAssignment(id, studentId);

            if (studentAssignment == null)
            {
                studentAssignment = new StudentAssignmentResult
                {
                    AssignmentId = id,
                    StudentId = studentId
                };
            }

            return View("Submit", studentAssignment);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(StudentAssignmentResult model)
        {
            if (!ModelState.IsValid)
            {
                return View("Submit", model);
            }

            try
            {
                var result = await _services.AssignmentService.SubmitAssignment(model);
                if (result != null && !string.IsNullOrEmpty(result.Id))
                {
                    TempData["SuccessMessage"] = "Assignment submitted successfully!";
                    return RedirectToAction("Details", new { id = model.AssignmentId });
                }

                ModelState.AddModelError("", "Failed to submit assignment. Please try again.");
                return View("Submit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while submitting assignment: {ex.Message}");
                return View("Submit", model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Submissions(string id)
        {
            var submissions = await _services.AssignmentService.GetAssignmentSubmissions(id);
            ViewBag.AssignmentId = id;
            return View("Submissions", submissions);
        }

        // NEW: Action for filtering submissions by status
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> SubmissionsByStatus(string id, string status)
        {
            var submissions = await _services.AssignmentService.GetSubmissionsByStatus(id, status);
            ViewBag.AssignmentId = id;
            ViewBag.Status = status;
            return View("Submissions", submissions);
        }


        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Grade(string id)
        {
            var submission = await _services.AssignmentService.GetStudentAssignmentById(id);
            if (submission == null)
                return NotFound();

            return View("Grade", submission);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Grade(StudentAssignmentResult model)
        {
            if (!ModelState.IsValid)
            {
                return View("Grade", model);
            }

            try
            {
                var result = await _services.AssignmentService.GradeAssignment(model);
                if (result != null && !string.IsNullOrEmpty(result.Id))
                {
                    TempData["SuccessMessage"] = "Assignment graded successfully!";
                    return RedirectToAction("Submissions", new { id = result.AssignmentId });
                }

                ModelState.AddModelError("", "Failed to grade assignment. Please try again.");
                return View("Grade", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error while grading assignment: {ex.Message}");
                return View("Grade", model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MySubmissions()
        {
            var studentId = _tokenService.GetUserId();
            // This would need a new method in service to get all student submissions
            // For now, return view - implementation depends on your requirements
            return View("MySubmissions");
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var assignment = await _services.AssignmentService.GetAssignmentById(id);
                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Assignment not found.";
                    return RedirectToAction("Index", "Course");
                }

                var result = await _services.AssignmentService.DeleteAssignment(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Assignment deleted successfully!";
                    return RedirectToAction("Details", "Course", new { id = assignment.CourseId });
                }

                TempData["ErrorMessage"] = "Failed to delete assignment. Please try again.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error while deleting assignment: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}