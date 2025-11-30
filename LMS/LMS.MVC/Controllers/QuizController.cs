using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IUnitOfServices _services;

        public QuizController(IUnitOfServices services)
        {
            _services = services;
        }

        private async Task<bool> CheckEnrollmentAccess(string courseId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;
            return await _services.EnrollmentService.IsApprovedEnrollmentAsync(userId, courseId);
        }

        // GET: Quiz/Index
        [HttpGet]
        public async Task<IActionResult> Index(string courseId)
        {
            try
            {
                var serviceResult = await _services.QuizService.GetQuizzesByCourseAsync(courseId);
                if (!serviceResult.Success)
                {
                    TempData["Error"] = $"Error loading quizzes: {serviceResult.Message}";
                    return RedirectToAction("Index", "Course");
                }

                var viewModel = new LMS.MVC.Models.ViewModels.Quiz.QuizListViewModel
                {
                    CourseId = courseId,
                    Quizzes = serviceResult.Data ?? new List<QuizItemViewModel>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quizzes: {ex.Message}";
                return RedirectToAction("Index", "Course");
            }
        }

        // GET: Quiz/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var result = await _services.QuizService.GetQuizByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Quiz not found.";
                    return RedirectToAction("Index");
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quiz: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Quiz/Create
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Create(string courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: Quiz/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            // Set InstructorId from current user
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            model.InstructorId = instructorId;

            // Set temporary QuizId for questions to satisfy backend validation if needed
            // The backend service should overwrite this with the real ID
            foreach (var question in model.Questions)
            {
                question.QuizId = "TEMP";
            }

            // Remove validation errors for these fields since we just set them
            ModelState.Remove("InstructorId");
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions") && k.EndsWith("QuizId")).ToList())
            {
                ModelState.Remove(key);
            }

            // Remove validation for question type-specific fields
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var question = model.Questions[i];
                
                // For True/False questions, remove validation for options C and D
                if (question.Type == "TrueFalse")
                {
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    
                    // Set empty values for C and D to satisfy DTO if needed
                    question.OptionC = "";
                    question.OptionD = "";
                }
                
                // For Short Answer questions, remove validation for all options
                if (question.Type == "ShortAnswer")
                {
                    ModelState.Remove($"Questions[{i}].OptionA");
                    ModelState.Remove($"Questions[{i}].OptionB");
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    ModelState.Remove($"Questions[{i}].CorrectAnswer");
                    
                    // Set empty values for all options
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Quiz Creation ModelState Invalid:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - Field: {state.Key}, Error: {error.ErrorMessage}, Exception: {error.Exception?.Message}");
                    }
                }
                return View(model);
            }

            try
            {
                var result = await _services.QuizService.CreateQuizAsync(model);
                if (result.Success)
                {
                    TempData["Success"] = "Quiz created successfully!";
                    return RedirectToAction("Index", new { courseId = model.CourseId });
                }
                
                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating quiz: {ex.Message}";
                return View(model);
            }
        }

        // GET: Quiz/Edit/5
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var result = await _services.QuizService.GetQuizByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Quiz not found.";
                    return RedirectToAction("Index");
                }

                // Map QuizItemViewModel to UpdateQuizViewModel
                var updateModel = new UpdateQuizViewModel
                {
                    Id = result.Data.Id,
                    CourseId = result.Data.CourseId,
                    Title = result.Data.Title,
                    Description = result.Data.Description,
                    DurationMinutes = result.Data.DurationMinutes,
                    PassingScore = result.Data.PassingScore,
                    NumberOfQuestions = result.Data.NumberOfQuestions,
                    InstructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    Questions = result.Data.Questions?.Select(q => new CreateQuestionViewModel
                    {
                        Id = q.Id,  // Include question ID for updates
                        Text = q.Text,
                        OptionA = q.OptionA ?? "",
                        OptionB = q.OptionB ?? "",
                        OptionC = q.OptionC ?? "",
                        OptionD = q.OptionD ?? "",
                        CorrectAnswer = ParseCorrectAnswer(q.CorrectAnswer),
                        Points = q.Points,
                        Type = DetermineQuestionType(q),
                        QuizId = q.QuizId
                    }).ToList() ?? new List<CreateQuestionViewModel>()
                };

                return View(updateModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quiz: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private Domain.Enums.Options ParseCorrectAnswer(string correctAnswer)
        {
            return correctAnswer switch
            {
                "OptionA" => Domain.Enums.Options.OptionA,
                "OptionB" => Domain.Enums.Options.OptionB,
                "OptionC" => Domain.Enums.Options.OptionC,
                "OptionD" => Domain.Enums.Options.OptionD,
                _ => Domain.Enums.Options.OptionA
            };
        }

        private string DetermineQuestionType(LMS.BusinessLogic.DTOs.Question.ReadQuestionDTO question)
        {
            // Check for Short Answer FIRST (no options at all)
            if (string.IsNullOrEmpty(question.OptionA) && string.IsNullOrEmpty(question.OptionB) &&
                string.IsNullOrEmpty(question.OptionC) && string.IsNullOrEmpty(question.OptionD))
            {
                return "ShortAnswer";
            }
            // If no options C and D but has A and B, it's True/False
            if (string.IsNullOrEmpty(question.OptionC) && string.IsNullOrEmpty(question.OptionD))
            {
                return "TrueFalse";
            }
            // Otherwise it's Multiple Choice
            return "MultipleChoice";
        }

        // POST: Quiz/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(string id, UpdateQuizViewModel model)
        {
            // Remove validation for question type-specific fields (same as Create)
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var question = model.Questions[i];
                
                // For True/False questions, remove validation for options C and D
                if (question.Type == "TrueFalse")
                {
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    question.OptionC = "";
                    question.OptionD = "";
                }
                
                // For Short Answer questions, remove validation for all options
                if (question.Type == "ShortAnswer")
                {
                    ModelState.Remove($"Questions[{i}].OptionA");
                    ModelState.Remove($"Questions[{i}].OptionB");
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    ModelState.Remove($"Questions[{i}].CorrectAnswer");
                    
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Quiz Edit ModelState Invalid:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - Field: {state.Key}, Error: {error.ErrorMessage}, Exception: {error.Exception?.Message}");
                    }
                }
                return View(model);
            }

            try
            {
                var result = await _services.QuizService.UpdateQuizAsync(id, model);
                if (result.Success)
                {
                    TempData["Success"] = "Quiz updated successfully!";
                    return RedirectToAction("Details", new { id });
                }
                
                Console.WriteLine($"❌ Quiz Update Failed: {result.Message}");
                TempData["Error"] = $"Failed to update quiz: {result.Message}";
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Quiz Update Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                
                TempData["Error"] = $"Error updating quiz: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" | Inner: {ex.InnerException.Message}";
                }
                return View(model);
            }
        }

        // GET: Quiz/Take/5
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Take(string id)
        {
            try
            {
                // Start the quiz first
                var startResult = await _services.QuizService.StartQuizAsync(id);
                if (!startResult.Success)
                {
                    TempData["Error"] = startResult.Message ?? "Could not start quiz.";
                    return RedirectToAction("Index");
                }

                var result = await _services.QuizService.GetQuizForTakingAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Quiz not found or not available.";
                    return RedirectToAction("Index");
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quiz: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Quiz/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(SubmitQuizViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid submission.";
                return RedirectToAction("Take", new { id = model.QuizId });
            }

            try
            {
                var result = await _services.QuizService.SubmitQuizAsync(model);
                if (result.Success && result.Data != null)
                {
                    TempData["Success"] = "Quiz submitted successfully!";
                    // Store the result in TempData to pass to Results view
                    TempData["QuizResult"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                    return RedirectToAction("Results", new { id = model.QuizId });
                }
                
                TempData["Error"] = result.Message;
                return RedirectToAction("Take", new { id = model.QuizId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting quiz: {ex.Message}";
                return RedirectToAction("Take", new { id = model.QuizId });
            }
        }

        // GET: Quiz/Results/5
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Results(string id)
        {
            try
            {
                // Try to get result from TempData first (just submitted)
                if (TempData["QuizResult"] is string resultJson)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<QuizResultViewModel>(resultJson);
                    if (result != null)
                    {
                        return View(result);
                    }
                }

                // Otherwise fetch from API
                var apiResult = await _services.QuizService.GetQuizResultsAsync(id, User.Identity.Name);
                if (!apiResult.Success || apiResult.Data == null)
                {
                    TempData["Error"] = apiResult.Message ?? "Results not found.";
                    return RedirectToAction("Index");
                }
                return View(apiResult.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading results: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Quiz/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _services.QuizService.DeleteQuizAsync(id);
                if (result)
                {
                    TempData["Success"] = "Quiz deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete quiz.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting quiz: {ex.Message}";
            }
            
            return RedirectToAction("InstructorQuizzes");
        }

        // GET: Quiz/MyQuizzes
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyQuizzes()
        {
            try
            {
                var result = await _services.QuizService.GetMyQuizzesAsync();
                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Error loading quizzes.";
                    return RedirectToAction("Index", "Home");
                }

                return View(result.Data ?? new List<QuizItemViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quizzes: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Quiz/InstructorQuizzes
        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> InstructorQuizzes()
        {
            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    TempData["Error"] = "Unable to identify instructor.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _services.QuizService.GetQuizzesByInstructorAsync(instructorId);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Error loading quizzes.";
                    return View(new List<QuizItemViewModel>());
                }

                return View(result.Data ?? new List<QuizItemViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quizzes: {ex.Message}";
                return View(new List<QuizItemViewModel>());
            }
        }
    }
}
