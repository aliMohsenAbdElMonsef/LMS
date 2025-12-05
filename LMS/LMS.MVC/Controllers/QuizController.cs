using LMS.MVC.Models.ViewModels.Quiz;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LMS.BusinessLogic.DTOs.Quiz;


namespace LMS.MVC.Controllers
{
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


        [HttpGet]
        public async Task<IActionResult> Index(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {

                if (TempData["Error"] != null)
                {
                    return RedirectToAction("Index", "Course");
                }
                return BadRequest("Course ID is required.");
            }

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
                    Quizzes = (serviceResult.Data ?? new List<QuizItemViewModel>()).OrderBy(q => q.EndDate).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quizzes: {ex.Message}";
                return RedirectToAction("Index", "Course");
            }
        }


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

                if (User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
                {
                    if (!await CheckEnrollmentAccess(result.Data.CourseId))
                    {
                        TempData["Error"] = "You must be enrolled in the course to view this quiz.";
                        return RedirectToAction("Details", "Course", new { id = result.Data.CourseId });
                    }
                }


                if (User.Identity.IsAuthenticated && User.IsInRole("Student"))
                {
                    var statusResult = await _services.QuizService.GetQuizStatusAsync(id);
                    if (statusResult.Success && statusResult.Data != null)
                    {
                        result.Data.IsCompleted = statusResult.Data.Status == Domain.Enums.QuizStatus.Completed || statusResult.Data.Status == Domain.Enums.QuizStatus.Graded;
                        result.Data.AchievedScore = statusResult.Data.Grade ?? 0;
                    }
                }
                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading quiz: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create(string courseId)
        {
            if (!User.IsInRole("Admin"))
            {
                if (!await CheckEnrollmentAccess(courseId))
                {
                    TempData["Error"] = "You must be enrolled in the course to create a quiz.";
                    return RedirectToAction("Details", "Course", new { id = courseId });
                }
            }
            ViewBag.CourseId = courseId;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                if (!await CheckEnrollmentAccess(model.CourseId))
                {
                    TempData["Error"] = "You must be enrolled in the course to create a quiz.";
                    return RedirectToAction("Details", "Course", new { id = model.CourseId });
                }
            }


            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            model.InstructorId = instructorId;


            foreach (var question in model.Questions)
            {
                question.QuizId = "TEMP";
            }


            ModelState.Remove("InstructorId");
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Questions") && k.EndsWith("QuizId")).ToList())
            {
                ModelState.Remove(key);
            }


            for (int i = 0; i < model.Questions.Count; i++)
            {
                var question = model.Questions[i];
                

                if (question.Type == "TrueFalse")
                {
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    

                    question.OptionC = "";
                    question.OptionD = "";
                }
                

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

                if (!User.IsInRole("Admin"))
                {
                    if (!await CheckEnrollmentAccess(result.Data.CourseId))
                    {
                        TempData["Error"] = "You must be enrolled in the course to edit this quiz.";
                        return RedirectToAction("Details", "Course", new { id = result.Data.CourseId });
                    }
                }


                var updateModel = new UpdateQuizViewModel
                {
                    Id = result.Data.Id,
                    CourseId = result.Data.CourseId,
                    Title = result.Data.Title,
                    Description = result.Data.Description,
                    DurationMinutes = result.Data.DurationMinutes,
                    PassingScore = result.Data.PassingScore,
                    MaxAttempts = result.Data.MaxAttempts,
                    NumberOfQuestions = result.Data.NumberOfQuestions,
                    StartDate = result.Data.StartDate,
                    EndDate = result.Data.EndDate,
                    InstructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    Questions = result.Data.Questions?.Select(q => new CreateQuestionViewModel
                    {
                        Id = q.Id,
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
            if (string.IsNullOrEmpty(question.OptionA) && string.IsNullOrEmpty(question.OptionB) &&
                string.IsNullOrEmpty(question.OptionC) && string.IsNullOrEmpty(question.OptionD))
            {
                return "ShortAnswer";
            }
            if (string.IsNullOrEmpty(question.OptionC) && string.IsNullOrEmpty(question.OptionD))
            {
                return "TrueFalse";
            }

            return "MultipleChoice";
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(string id, UpdateQuizViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                if (!await CheckEnrollmentAccess(model.CourseId))
                {
                    TempData["Error"] = "You must be enrolled in the course to edit this quiz.";
                    return RedirectToAction("Details", "Course", new { id = model.CourseId });
                }
            }
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var question = model.Questions[i];
                
                if (question.Type == "TrueFalse")
                {
                    ModelState.Remove($"Questions[{i}].OptionC");
                    ModelState.Remove($"Questions[{i}].OptionD");
                    question.OptionC = "";
                    question.OptionD = "";
                }
                
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
                
                TempData["Error"] = $"Failed to update quiz: {result.Message}";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating quiz: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" | Inner: {ex.InnerException.Message}";
                }
                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Take(string id)
        {
            try
            {

                var quizResult = await _services.QuizService.GetQuizByIdAsync(id);
                if (!quizResult.Success || quizResult.Data == null)
                {
                    TempData["Error"] = "Quiz not found.";
                    return RedirectToAction("Index", "Home");
                }

                if (!await CheckEnrollmentAccess(quizResult.Data.CourseId))
                {
                    TempData["Error"] = "You must be enrolled in the course to take this quiz.";
                    return RedirectToAction("Details", "Course", new { id = quizResult.Data.CourseId });
                }


                var startResult = await _services.QuizService.StartQuizAsync(id);
                if (!startResult.Success)
                {
                    TempData["Error"] = startResult.Message ?? "Could not start quiz.";
                    return RedirectToAction("Index");
                }

                var result = await _services.QuizService.GetQuizForTakingAsync(id);
                if (!result.Success || result.Data == null)
                {
                    if (result.Message != null && result.Message.Contains("You have already completed this quiz"))
                    {
                        TempData["Error"] = "You have already completed this quiz. You cannot take it again.";
                    }
                    else
                    {
                        TempData["Error"] = result.Message ?? "Quiz not found or not available.";
                    }
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(SubmitQuizViewModel model)
        {

            model.StudentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(model.StudentId))
            {
                TempData["Error"] = "Unable to identify student. Please log in again.";
                return RedirectToAction("Take", new { id = model.QuizId });
            }
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


        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Results(string id)
        {
            try
            {
                if (TempData["QuizResult"] is string resultJson)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<QuizResultViewModel>(resultJson);
                    if (result != null)
                    {
                        return View(result);
                    }
                }

                var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var apiResult = await _services.QuizService.GetStudentQuizResultAsync(id, studentId);
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var quiz = await _services.QuizService.GetQuizByIdAsync(id);
                    if (quiz.Success && quiz.Data != null)
                    {
                        if (!await CheckEnrollmentAccess(quiz.Data.CourseId))
                        {
                            TempData["Error"] = "You must be enrolled in the course to delete this quiz.";
                            return RedirectToAction("InstructorQuizzes");
                        }
                    }
                }

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
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Submissions(string id)
        {
            try
            {
                var result = await _services.QuizService.GetQuizSubmissionsAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Error loading submissions.";
                    return RedirectToAction("Details", new { id });
                }

                var quiz = await _services.QuizService.GetQuizByIdAsync(id);
                
                if (quiz.Success && quiz.Data != null && !User.IsInRole("Admin"))
                {
                    if (!await CheckEnrollmentAccess(quiz.Data.CourseId))
                    {
                        TempData["Error"] = "You must be enrolled in the course to view submissions.";
                        return RedirectToAction("Details", "Course", new { id = quiz.Data.CourseId });
                    }
                }

                ViewBag.QuizTitle = quiz.Data?.Title ?? "Quiz";
                ViewBag.QuizId = id;

                return View(result.Data ?? new List<LMS.BusinessLogic.DTOs.Quiz.QuizSubmissionDTO>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading submissions: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Grade(string quizId, string studentId)
        {
            var result = await _services.QuizService.GetStudentQuizResultAsync(quizId, studentId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message ?? "Error loading student result.";
                return RedirectToAction("Submissions", new { id = quizId });
            }

            if (!User.IsInRole("Admin"))
            {
                if (!await CheckEnrollmentAccess(result.Data.CourseId))
                {
                    TempData["Error"] = "You must be enrolled in the course to grade this quiz.";
                    return RedirectToAction("Details", "Course", new { id = result.Data.CourseId });
                }
            }


            var questionsToGrade = result.Data.QuestionResults
                .Where(q => q.CorrectAnswer == "Requires Manual Grading")
                .Select(q => new QuestionGradeViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    StudentAnswer = q.SelectedAnswer,
                    CorrectAnswer = q.CorrectAnswer,
                    IsCorrect = q.IsCorrect,
                    Points = q.Points
                }).ToList();


            if (!questionsToGrade.Any())
            {
                TempData["Info"] = "This quiz has been fully auto-graded. No manual grading required.";
                return RedirectToAction("Submissions", new { id = quizId });
            }

            var viewModel = new ManualGradeViewModel
            {
                QuizId = result.Data.QuizId,
                StudentId = studentId,
                QuizTitle = result.Data.QuizTitle,
                Questions = questionsToGrade
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(ManualGradeViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                var quiz = await _services.QuizService.GetQuizByIdAsync(model.QuizId);
                if (quiz.Success && quiz.Data != null)
                {
                    if (!await CheckEnrollmentAccess(quiz.Data.CourseId))
                    {
                        TempData["Error"] = "You must be enrolled in the course to grade this quiz.";
                        return RedirectToAction("Details", "Course", new { id = quiz.Data.CourseId });
                    }
                }
            }

            var dto = new LMS.BusinessLogic.DTOs.Quiz.ManualGradeDTO
            {
                QuizId = model.QuizId,
                StudentId = model.StudentId,
                Grades = model.Questions.Select(q => new LMS.BusinessLogic.DTOs.Quiz.QuestionGradeDTO
                {
                    QuestionId = q.QuestionId,
                    IsCorrect = q.IsCorrect
                }).ToList()
            };

            var result = await _services.QuizService.GradeQuizAsync(dto);
            if (result.Success)
            {
                TempData["Success"] = "Quiz graded successfully.";
                return RedirectToAction("Submissions", new { id = model.QuizId });
            }

            TempData["Error"] = result.Message ?? "Failed to grade quiz.";
            return View(model);
        }
    }
}
