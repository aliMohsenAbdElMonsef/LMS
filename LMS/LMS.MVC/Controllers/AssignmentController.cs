using LMS.Entity.Enums;
using LMS.MVC.Models.ViewModels.Assignment;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create(ReadAssignmentResult model, IFormFile assignmentFile)
        {
            Console.WriteLine("=== CREATE ASSIGNMENT POST ===");
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"CourseId: {model.CourseId}");
            Console.WriteLine($"File: {assignmentFile?.FileName ?? "NULL"}");
            Console.WriteLine($"File Size: {assignmentFile?.Length ?? 0} bytes");
            ModelState.Remove("FilePath");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View("Create", model);
            }

            if (assignmentFile == null || assignmentFile.Length == 0)
            {
                Console.WriteLine("❌ No file uploaded");
                ModelState.AddModelError("", "Please select an assignment file.");
                return View("Create", model);
            }

            try
            {
                Console.WriteLine("✅ Calling CreateAssignmentWithFile...");

                var createdAssignment = await _services.AssignmentService.CreateAssignmentWithFile(
                    model, assignmentFile);

                Console.WriteLine($"✅ Assignment created with ID: {createdAssignment?.Id}");

                if (createdAssignment != null && !string.IsNullOrEmpty(createdAssignment.Id))
                {
                    TempData["SuccessMessage"] = "Assignment created successfully!";
                    return RedirectToAction("Details", new { id = createdAssignment.Id });
                }

                Console.WriteLine("❌ Creation failed - returned null or empty ID");
                ModelState.AddModelError("", "Failed to create assignment. Please try again.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Create", model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var assignment = await _services.AssignmentService.GetAssignmentById(id);
            if (assignment == null)
                return NotFound();

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
                return View("Edit", model);
            }

            try
            {
                var updatedAssignment = await _services.AssignmentService.EditAssignment(model);
                if (updatedAssignment != null)
                {
                    TempData["SuccessMessage"] = "Assignment updated successfully!";
                    return RedirectToAction("Details", new { id = updatedAssignment.Id });
                }

                ModelState.AddModelError("", "Failed to update assignment.");
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating assignment: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
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
        public async Task<IActionResult> Submit(StudentAssignmentResult model, IFormFile submissionFile)
        {
            Console.WriteLine("=== SUBMIT ASSIGNMENT POST ===");
            Console.WriteLine($"AssignmentId: {model.AssignmentId}");
            Console.WriteLine($"StudentId: {model.StudentId}");
            Console.WriteLine($"File: {submissionFile?.FileName ?? "NULL"}");
            Console.WriteLine($"File Size: {submissionFile?.Length ?? 0} bytes");

            // ✅ ADD ALL THESE LINES
            ModelState.Remove("FilePath");
            ModelState.Remove("Status");
            ModelState.Remove("StudentName");
            ModelState.Remove("StudentAssignmentId");
            ModelState.Remove("Id");
            ModelState.Remove("Grade");
            ModelState.Remove("GradedAt");
            ModelState.Remove("SubmittedAt");
            ModelState.Remove("Feedback");

            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View("Submit", model);
            }

            if (submissionFile == null || submissionFile.Length == 0)
            {
                Console.WriteLine("❌ No file selected");
                ModelState.AddModelError("", "Please select a submission file.");
                return View("Submit", model);
            }

            try
            {
                Console.WriteLine("✅ Calling SubmitAssignmentWithFile...");

                var result = await _services.AssignmentService.SubmitAssignmentWithFile(
                    model.AssignmentId, model.StudentId, submissionFile);

                Console.WriteLine($"✅ Result: {(result != null ? "Success" : "NULL")}");

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment submitted successfully!";
                    return RedirectToAction("Details", new { id = model.AssignmentId });
                }

                Console.WriteLine("❌ Result was null");
                ModelState.AddModelError("", "Failed to submit assignment.");
                return View("Submit", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
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
        [Authorize]
        public async Task<IActionResult> DownloadAssignment(string id)
        {
            try
            {
                var assignment = await _services.AssignmentService.GetAssignmentById(id);
                if (assignment == null || string.IsNullOrEmpty(assignment.FilePath))
                {
                    TempData["ErrorMessage"] = "File not found.";
                    return RedirectToAction("Details", new { id });
                }

                // Download from API
                var fileResult = await _services.AssignmentService.DownloadFileFromApi(assignment.FilePath);
                if (fileResult == null)
                {
                    TempData["ErrorMessage"] = "File not accessible.";
                    return RedirectToAction("Details", new { id });
                }

                return fileResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error downloading file: {ex.Message}");
                TempData["ErrorMessage"] = "Error downloading file.";
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadSubmission(string studentAssignmentId)
        {
            try
            {
                var submission = await _services.AssignmentService.GetStudentAssignmentById(studentAssignmentId);
                if (submission == null || string.IsNullOrEmpty(submission.FilePath))
                {
                    TempData["ErrorMessage"] = "File not found.";
                    return RedirectToAction("Index", "Course");
                }

                // Download from API
                var fileResult = await _services.AssignmentService.DownloadFileFromApi(submission.FilePath);
                if (fileResult == null)
                {
                    TempData["ErrorMessage"] = "File not accessible.";
                    return RedirectToAction("Index", "Course");
                }

                return fileResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error downloading file: {ex.Message}");
                TempData["ErrorMessage"] = "Error downloading file.";
                return RedirectToAction("Index", "Course");
            }
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
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment graded successfully!";
                    return RedirectToAction("Submissions", new { id = result.AssignmentId });
                }

                ModelState.AddModelError("", "Failed to grade assignment.");
                return View("Grade", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error grading assignment: {ex.Message}");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Grade", model);
            }
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

                // API will handle file deletion
                var result = await _services.AssignmentService.DeleteAssignment(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Assignment deleted successfully!";
                    return RedirectToAction("Details", "Course", new { id = assignment.CourseId });
                }

                TempData["ErrorMessage"] = "Failed to delete assignment.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting assignment: {ex.Message}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}