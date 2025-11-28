
using LMS.Entity.Enums;
using LMS.MVC.Models.ViewModels.Assignment;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

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
            model.DueDate = DateTime.UtcNow
                .AddSeconds(-DateTime.Now.Second)
                .AddMilliseconds(-DateTime.Now.Millisecond);

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
                Console.WriteLine("? ModelState Errors:");
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
                Console.WriteLine("? No file uploaded");
                ModelState.AddModelError("", "Please select an assignment file.");
                return View("Create", model);
            }

            try
            {
                Console.WriteLine("? Calling CreateAssignmentWithFile...");

                var createdAssignment = await _services.AssignmentService.CreateAssignmentWithFile(
                    model, assignmentFile);

                Console.WriteLine($"? Assignment created with ID: {createdAssignment?.Id}");

                if (createdAssignment != null && !string.IsNullOrEmpty(createdAssignment.Id))
                {
                    TempData["SuccessMessage"] = "Assignment created successfully!";
                    return RedirectToAction("Details", new { id = createdAssignment.Id });
                }

                Console.WriteLine("? Creation failed - returned null or empty ID");
                ModelState.AddModelError("", "Failed to create assignment. Please try again.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Exception: {ex.Message}");
                Console.WriteLine($"? Stack Trace: {ex.StackTrace}");
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

            return View(assignment);
        }

                [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(string id)
        {
            var assignment = await _services.AssignmentService.GetEditModel(id);
            if (assignment == null)
                return NotFound();

            return View(assignment);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(ReadAssignmentResult model)
        {
            Console.WriteLine("=== EDIT ASSIGNMENT POST ===");
            Console.WriteLine($"Id: {model.Id}");
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"DueDate: {model.DueDate}");
            Console.WriteLine($"CourseId: {model.CourseId}");
            Console.WriteLine($"FilePath: {model.FilePath}");
            
            // Remove validation for fields that are not editable
            ModelState.Remove("FilePath");
            ModelState.Remove("UploadDate");
            ModelState.Remove("SubmissionsCount");
            ModelState.Remove("CourseName");
            ModelState.Remove("InstructorName");
            ModelState.Remove("InstructorId");
            
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
                return View(model);
            }

            var result = await _services.AssignmentService.EditAssignment(model);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Assignment updated successfully!";
                return RedirectToAction("Details", new { id = model.Id });
            }

            ModelState.AddModelError("", "Failed to update assignment.");
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Submissions(string id, string status = null)
        {
            var submissions = await _services.AssignmentService.GetAssignmentSubmissions(id);

            if (!string.IsNullOrEmpty(status))
            {
                submissions = status switch
                {
                    "PendingGrading" => submissions.Where(s => s.IsPending),
                    "Graded" => submissions.Where(s => s.IsGraded),
                    "NotSubmitted" => submissions.Where(s => !s.IsSubmitted),
                    _ => submissions
                };
            }

            ViewBag.AssignmentId = id;
            ViewBag.Status = status;
            return View(submissions);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(string id)
        {
            var studentId = _tokenService.GetUserId();
            var submission = await _services.AssignmentService.GetStudentAssignment(id, studentId);

            if (submission != null && submission.IsSubmitted)
            {
                TempData["InfoMessage"] = "You have already submitted this assignment.";
                return RedirectToAction("Details", new { id });
            }

            var assignment = await _services.AssignmentService.GetAssignmentById(id);
            if (assignment != null && assignment.DueDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This assignment is overdue and cannot be submitted.";
                return RedirectToAction("Details", new { id });
            }

            return View(new StudentAssignmentResult { AssignmentId = id });
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(StudentAssignmentResult model, IFormFile submissionFile)
        {
            if (submissionFile == null || submissionFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View(model);
            }

            var assignment = await _services.AssignmentService.GetAssignmentById(model.AssignmentId);
            if (assignment != null && assignment.DueDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This assignment is overdue and cannot be submitted.";
                return RedirectToAction("Details", new { id = model.AssignmentId });
            }

            try
            {
                var studentId = _tokenService.GetUserId();
                var result = await _services.AssignmentService.SubmitAssignmentWithFile(model.AssignmentId, studentId, submissionFile);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment submitted successfully!";
                    return RedirectToAction("Details", new { id = model.AssignmentId });
                }

                ModelState.AddModelError("", "Failed to submit assignment.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                var assignment = await _services.AssignmentService.GetAssignmentById(id);
                if (assignment == null || string.IsNullOrEmpty(assignment.FilePath))
                {
                    TempData["ErrorMessage"] = "File not found.";
                    return RedirectToAction("Details", new { id });
                }

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
                Console.WriteLine($"? Error downloading file: {ex.Message}");
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
                Console.WriteLine($"? Error downloading file: {ex.Message}");
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
            Console.WriteLine("=== GRADE ASSIGNMENT POST ===");
            Console.WriteLine($"StudentAssignmentId: {model.Id}");
            Console.WriteLine($"Grade: {model.Grade}");
            Console.WriteLine($"Feedback: {model.Feedback}");

            ModelState.Remove("FilePath");
            ModelState.Remove("Status");
            ModelState.Remove("StudentName");
            ModelState.Remove("AssignmentId");
            ModelState.Remove("StudentId");
            ModelState.Remove("SubmittedAt");
            ModelState.Remove("GradedAt");
            ModelState.Remove("IsSubmitted");
            ModelState.Remove("StudentAssignmentId");
            Console.WriteLine($"ModelState Valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("? ModelState Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View("Grade", model);
            }

            try
            {
                Console.WriteLine("? Calling GradeAssignment...");

                var result = await _services.AssignmentService.GradeAssignment(model);

                Console.WriteLine($"? Result: {(result != null ? "Success" : "NULL")}");

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Assignment graded successfully!";
                    return RedirectToAction("Submissions", new { id = result.AssignmentId });
                }

                Console.WriteLine("? Result was null");
                ModelState.AddModelError("", "Failed to grade assignment. Please try again.");
                return View("Grade", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error grading assignment: {ex.Message}");
                Console.WriteLine($"? Stack: {ex.StackTrace}");
                ModelState.AddModelError("", $"Error while grading assignment: {ex.Message}");
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
                    return RedirectToAction("InstructorAssignments");
                }

                var result = await _services.AssignmentService.DeleteAssignment(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Assignment deleted successfully!";
                    
                    // Check if the request came from InstructorAssignments page
                    var referer = Request.Headers["Referer"].ToString();
                    if (referer.Contains("InstructorAssignments", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("InstructorAssignments");
                    }
                    
                    return RedirectToAction("Details", "Course", new { id = assignment.CourseId });
                }

                TempData["ErrorMessage"] = "Failed to delete assignment.";
                return RedirectToAction("InstructorAssignments");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting assignment: {ex.Message}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("InstructorAssignments");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MySubmissions()
        {
            var studentId = _tokenService.GetUserId();
            var submissions = await _services.AssignmentService.GetStudentSubmissions(studentId);
            return View("MySubmissions", submissions);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyAssignments(string filter = "all")
        {
            var studentId = _tokenService.GetUserId();
            var allAssignments = await _services.AssignmentService.GetStudentAllAssignments(studentId);

            IEnumerable<StudentAssignmentItemResult> filteredAssignments = filter switch
            {
                "notsubmitted" => allAssignments.Where(a => !a.IsSubmitted),
                "pending" => allAssignments.Where(a => a.IsSubmitted && !a.Grade.HasValue),
                "graded" => allAssignments.Where(a => a.Grade.HasValue),
                "overdue" => allAssignments.Where(a => a.DueDate < DateTime.Now && !a.IsSubmitted),
                _ => allAssignments
            };

            var sortedAssignments = filteredAssignments
                .OrderBy(a => {
                    if (a.Grade.HasValue) return 3; 

                    if (!a.IsSubmitted)
                    {
                        if (a.DueDate < DateTime.Now)
                            return 2; 
                        else
                            return 0; 
                    }

                    return 1; 
                })
                .ThenBy(a => a.DueDate) 
                .ToList();

            var viewModel = new MyAssignmentsViewModel
            {
                Assignments = sortedAssignments
            };

            ViewBag.Filter = filter;
            return View("MyAssignments", viewModel);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorAssignments()
        {
            var instructorId = _tokenService.GetUserId();
            var response = await _services.AssignmentService.GetAssignmentsByInstructorAsync(instructorId);

            if (!response.Success)
            {
                TempData["Error"] = response.Message;
                return RedirectToAction("Index", "Course");
            }

            return View(response.Data);
        }
    }
}
