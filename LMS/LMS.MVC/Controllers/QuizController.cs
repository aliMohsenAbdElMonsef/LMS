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
            if (!ModelState.IsValid)
            {
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
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quiz: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Quiz/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(string id, UpdateQuizViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                
                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating quiz: {ex.Message}";
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

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyQuizzes()
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Error"] = "Unable to identify student.";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _services.QuizService.GetQuizzesByStudentAsync(studentId);
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
