using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Question;
using LMS.BusinessLogic.DTOs.Quiz;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.DTOs.Notification;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class QuizServices : BaseServices<Quiz, ReadQuizDTO, CreateQuizDTO, UpdateQuizDTO>, IQuizServices
    {
        private readonly INotificationService _notificationService;

        public QuizServices(IUnitOfWork unitOfWork, INotificationService notificationService) : base(unitOfWork)
        {
            _notificationService = notificationService;
        }

        protected override string GetIdFromUpdateDTO(UpdateQuizDTO dto) => dto.Id;

        protected override IBaseRepository<Quiz, string> GetRepo() => _unitOfWork.Quizzes;

        protected override Quiz MapToEntity(CreateQuizDTO dto)
        {
            var quizId = Guid.NewGuid().ToString();
            var quiz = new Quiz
            {
                Id = quizId,
                Title = dto.Title,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PassingScore = dto.PassingScore,
                MaxAttempts = dto.MaxAttempts,
                CourseId = dto.CourseId,
                InstructorId = dto.InstructorId,
                NumberOfQuestions = dto.Questions.Count
            };

            if (dto.Questions != null)
            {
                foreach (var qDto in dto.Questions)
                {
                    quiz.Questions.Add(new Question
                    {
                        Id = Guid.NewGuid().ToString(),
                        QuizId = quizId,
                        Text = qDto.Text,
                        OptionA = qDto.OptionA,
                        OptionB = qDto.OptionB,
                        OptionC = qDto.OptionC,
                        OptionD = qDto.OptionD,
                        CorrectAnswer = qDto.CorrectAnswer,
                        Points = qDto.Points
                    });
                }
            }

            return quiz;
        }

        protected override ReadQuizDTO MapToReadDTO(Quiz entity)
        {
            var totalQuestions = entity.Questions?.Count ?? 0;
            var nonDeletedQuestions = entity.Questions?.Count(q => !q.IsDeleted) ?? 0;
            var deletedQuestions = entity.Questions?.Count(q => q.IsDeleted) ?? 0;
            
            Console.WriteLine($"📊 MapToReadDTO for Quiz {entity.Id}:");
            Console.WriteLine($"   Total questions in collection: {totalQuestions}");
            Console.WriteLine($"   Non-deleted questions: {nonDeletedQuestions}");
            Console.WriteLine($"   Deleted questions: {deletedQuestions}");
            Console.WriteLine($"   Database NumberOfQuestions: {entity.NumberOfQuestions}");
            
            return new ReadQuizDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                DurationMinutes = entity.DurationMinutes,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                NumberOfQuestions = entity.Questions?.Count(q => !q.IsDeleted) ?? entity.NumberOfQuestions,
                PassingScore = entity.PassingScore,
                MaxAttempts = entity.MaxAttempts,
                CourseId = entity.CourseId,
                CourseName = entity.Course?.Name ?? "",
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor != null ? $"{entity.Instructor.FirstName} {entity.Instructor.LastName}" : "",
                Questions = entity.Questions?
                    .Where(q => !q.IsDeleted)  // Filter out deleted questions
                    .Select(q => new ReadQuestionDTO
                    {
                        Id = q.Id,
                        QuizId = q.QuizId,
                        Text = q.Text,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectAnswer = q.CorrectAnswer.ToString(),
                        Points = q.Points
                    }).ToList() ?? new List<ReadQuestionDTO>()
            };
        }

        protected override Quiz UpdateToEntity(UpdateQuizDTO dto, Quiz existingEntity)
        {
            Console.WriteLine($"🔄 UpdateToEntity called for quiz {existingEntity.Id}");
            Console.WriteLine($"Existing questions count: {existingEntity.Questions.Count}");
            Console.WriteLine($"DTO questions count: {dto.Questions?.Count ?? 0}");
            
            existingEntity.Title = dto.Title ?? existingEntity.Title;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.DurationMinutes = dto.DurationMinutes ?? existingEntity.DurationMinutes;
            existingEntity.StartDate = dto.StartDate ?? existingEntity.StartDate;
            existingEntity.EndDate = dto.EndDate ?? existingEntity.EndDate;
            existingEntity.PassingScore = dto.PassingScore ?? existingEntity.PassingScore;
            existingEntity.MaxAttempts = dto.MaxAttempts ?? existingEntity.MaxAttempts;
            existingEntity.InstructorId = dto.InstructorId ?? existingEntity.InstructorId;

            // Update questions
            if (dto.Questions != null && dto.Questions.Any())
            {
                // Get the list of question IDs from the DTO
                var dtoQuestionIds = dto.Questions
                    .Where(q => !string.IsNullOrEmpty(q.Id))
                    .Select(q => q.Id)
                    .ToHashSet();

                Console.WriteLine($"DTO Question IDs: {string.Join(", ", dtoQuestionIds)}");

                // Mark questions for deletion if they're not in the DTO
                foreach (var existingQuestion in existingEntity.Questions.Where(q => !q.IsDeleted).ToList())
                {
                    if (!dtoQuestionIds.Contains(existingQuestion.Id))
                    {
                        Console.WriteLine($"Marking question {existingQuestion.Id} as deleted");
                        existingQuestion.IsDeleted = true;
                        existingQuestion.DeletedAt = DateTime.UtcNow;
                    }
                }

                // Update existing questions and add new ones
                foreach (var qDto in dto.Questions)
                {
                    if (!string.IsNullOrEmpty(qDto.Id))
                    {
                        // Update existing question
                        var existingQuestion = existingEntity.Questions.FirstOrDefault(q => q.Id == qDto.Id && !q.IsDeleted);
                        if (existingQuestion != null)
                        {
                            Console.WriteLine($"Updating existing question {existingQuestion.Id}");
                            existingQuestion.Text = qDto.Text ?? existingQuestion.Text;
                            existingQuestion.OptionA = qDto.OptionA;
                            existingQuestion.OptionB = qDto.OptionB;
                            existingQuestion.OptionC = qDto.OptionC;
                            existingQuestion.OptionD = qDto.OptionD;
                            existingQuestion.CorrectAnswer = qDto.CorrectAnswer ?? existingQuestion.CorrectAnswer;
                            existingQuestion.Points = qDto.Points ?? existingQuestion.Points;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Question {qDto.Id} not found in existing questions!");
                        }
                    }
                    else
                    {
                        // Add new question (only if it doesn't already exist)
                        var newQuestionId = Guid.NewGuid().ToString();
                        Console.WriteLine($"Adding new question {newQuestionId}");
                        var newQuestion = new Question
                        {
                            Id = newQuestionId,
                            QuizId = existingEntity.Id,
                            Text = qDto.Text,
                            OptionA = qDto.OptionA,
                            OptionB = qDto.OptionB,
                            OptionC = qDto.OptionC,
                            OptionD = qDto.OptionD,
                            CorrectAnswer = qDto.CorrectAnswer ?? Options.OptionA,
                            Points = qDto.Points ?? 1
                        };
                        existingEntity.Questions.Add(newQuestion);
                    }
                }

                // Update question count (only count non-deleted questions)
                existingEntity.NumberOfQuestions = existingEntity.Questions.Count(q => !q.IsDeleted);
                Console.WriteLine($"Final question count: {existingEntity.NumberOfQuestions}");
            }
            
            return existingEntity;
        }

        public override async Task<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetAllAsync()
        {
            try
            {
                var entities = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .ToListAsync();

                ServiceResponseDTO<IEnumerable<ReadQuizDTO>> response = new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Data = entities.Select(e => MapToReadDTO(e)).ToList(),
                    Success = true,
                    Message = "Quizzes retrieved successfully."
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quizzes: {ex.Message}", ex);
            }
        }

        public override async Task<ServiceResponseDTO<ReadQuizDTO>> UpdateAsync(UpdateQuizDTO dto)
        {
            try
            {
                var id = dto.Id;

                // Eager load questions to ensure we can update them properly
                var existingEntity = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (existingEntity == null)
                    throw new Exception($"Quiz with id '{id}' not found.");

                Console.WriteLine($"📝 UpdateAsync: Found quiz with {existingEntity.Questions.Count} existing questions");

                var updatedEntity = UpdateToEntity(dto, existingEntity);
                await GetRepo().UpdateAsync(updatedEntity);
                await _unitOfWork.SaveChangesAsync();
                
                var ReadEntity = MapToReadDTO(updatedEntity);
                ServiceResponseDTO<ReadQuizDTO> response = new ServiceResponseDTO<ReadQuizDTO>
                {
                    Data = ReadEntity,
                    Success = true,
                    Message = "Quiz updated successfully."
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UpdateAsync Exception: {ex.Message}");
                throw new Exception($"Error updating quiz: {ex.Message}", ex);
            }
        }

        public override async Task<ServiceResponseDTO<ReadQuizDTO>> GetByIdAsync(string id)
        {
            try
            {
                var entity = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (entity == null)
                    return new ServiceResponseDTO<ReadQuizDTO> { Success = false, Message = "Quiz not found." };

                return new ServiceResponseDTO<ReadQuizDTO>
                {
                    Data = MapToReadDTO(entity),
                    Success = true,
                    Message = "Quiz retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<ReadQuizDTO>
                {
                    Success = false,
                    Message = $"Error retrieving quiz: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<bool>> StartQuizAsync(string quizId, string studentId)
        {
            try
            {
                var quiz = await GetRepo().GetQueryable()
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Quiz not found." };

                // Check availability
                bool isStarted = quiz.StartDate == DateTime.MinValue || DateTime.Now >= quiz.StartDate;
                bool isEnded = quiz.EndDate != DateTime.MinValue && DateTime.Now > quiz.EndDate;

                if (!isStarted || isEnded)
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Quiz is not currently available." };
                }

                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();

                // Check max attempts
                var completedAttempts = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId && 
                           (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .CountAsync();

                if (completedAttempts >= quiz.MaxAttempts)
                {
                    return new ServiceResponseDTO<bool> 
                    { 
                        Success = false, 
                        Message = $"You have reached the maximum number of attempts ({quiz.MaxAttempts}) for this quiz." 
                    };
                }

                // Check if already in progress
                var inProgressAttempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId && sq.Status == QuizStatus.InProgress);

                if (inProgressAttempt != null)
                {
                    return new ServiceResponseDTO<bool>
                    {
                        Data = true,
                        Success = true,
                        Message = "Resuming quiz attempt."
                    };
                }

                // Create new attempt
                var newAttempt = new StudentQuiz
                {
                    StudentId = studentId,
                    QuizId = quizId,
                    StartTime = DateTime.UtcNow,
                    Status = QuizStatus.InProgress
                };

                await studentQuizSet.AddAsync(newAttempt);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResponseDTO<bool>
                {
                    Data = true,
                    Success = true,
                    Message = "Quiz started successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<bool>
                {
                    Success = false,
                    Message = $"Error starting quiz: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<QuizAttemptDTO>> GetQuizForTakingAsync(string quizId, string studentId)
        {
            try
            {
                var quiz = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .Include(q => q.Course)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                    return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = "Quiz not found." };

                // Check availability
                // If StartDate is MinValue, it's available from the beginning
                // If EndDate is MinValue, it's available indefinitely
                bool isStarted = quiz.StartDate == DateTime.MinValue || DateTime.Now >= quiz.StartDate;
                bool isEnded = quiz.EndDate != DateTime.MinValue && DateTime.Now > quiz.EndDate;

                if (!isStarted || isEnded)
                {
                    var msg = $"Quiz is not currently available. Now: {DateTime.Now}, Start: {quiz.StartDate}, End: {quiz.EndDate}";
                    Console.WriteLine($"❌ {msg}");
                    return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = msg };
                }

                var studentQuizSet = _unitOfWork.GetQueryable<StudentQuiz>();
                
                // Get all completed attempts for this student and quiz
                var completedAttempts = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId && 
                           (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .CountAsync();

                // Check if student has exceeded max attempts
                if (completedAttempts >= quiz.MaxAttempts)
                {
                    return new ServiceResponseDTO<QuizAttemptDTO> 
                    { 
                        Success = false, 
                        Message = $"You have reached the maximum number of attempts ({quiz.MaxAttempts}) for this quiz." 
                    };
                }

                // Get current in-progress attempt if exists
                var attempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId && sq.Status == QuizStatus.InProgress);

                int timeRemaining = quiz.DurationMinutes * 60;
                DateTime startTime = DateTime.UtcNow;

                // Check timer for in-progress attempt
                if (attempt != null)
                {
                    startTime = attempt.StartTime;
                    var timeElapsed = DateTime.UtcNow - attempt.StartTime;
                    timeRemaining = (int)((quiz.DurationMinutes * 60) - timeElapsed.TotalSeconds);
                    
                    if (timeRemaining <= 0)
                    {
                        // Auto-submit logic could be triggered here or handled by frontend
                        return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = "Time expired." };
                    }
                }

                var attemptDto = new QuizAttemptDTO
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    DurationMinutes = quiz.DurationMinutes,
                    TimeRemainingSeconds = timeRemaining,
                    StartTime = startTime,
                    Questions = quiz.Questions?.Select(q => new ReadQuestionDTO
                    {
                        Id = q.Id,
                        QuizId = q.QuizId,
                        Text = q.Text,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectAnswer = "", // Don't send correct answer to client!
                        Points = q.Points
                    }).ToList() ?? new List<ReadQuestionDTO>()
                };

                return new ServiceResponseDTO<QuizAttemptDTO>
                {
                    Data = attemptDto,
                    Success = true,
                    Message = "Quiz retrieved."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<ServiceResponseDTO<QuizResultDTO>> SubmitQuizAsync(SubmitQuizDTO dto)
        {
            try
            {
                // Get quiz with questions
                var quiz = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .Include(q => q.Course)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

                if (quiz == null)
                {
                    return new ServiceResponseDTO<QuizResultDTO>
                    {
                        Success = false,
                        Message = "Quiz not found."
                    };
                }

                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                
                // Find the current in-progress attempt
                var attempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == dto.StudentId && sq.QuizId == dto.QuizId && sq.Status == QuizStatus.InProgress);

                if (attempt == null)
                {
                    // Create new attempt if missing (shouldn't happen normally)
                    attempt = new StudentQuiz
                    {
                        StudentId = dto.StudentId,
                        QuizId = dto.QuizId,
                        StartTime = DateTime.UtcNow,
                        Status = QuizStatus.InProgress
                    };
                    await studentQuizSet.AddAsync(attempt);
                }

                if (attempt.Status == QuizStatus.Completed || attempt.Status == QuizStatus.Graded)
                {
                     return new ServiceResponseDTO<QuizResultDTO> { Success = false, Message = "Quiz already submitted." };
                }

                // Grade the quiz
                    int correctAnswers = 0;
                    int totalPoints = 0;
                    int earnedPoints = 0;
                    var questionResults = new List<QuestionResultDTO>();

                    foreach (var question in quiz.Questions)
                    {
                        totalPoints += question.Points;
                        var studentAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                        bool isCorrect = studentAnswer != null && studentAnswer.SelectedAnswer == question.CorrectAnswer;
                        if (isCorrect)
                        {
                            correctAnswers++;
                            earnedPoints += question.Points;
                        }

                        questionResults.Add(new QuestionResultDTO
                        {
                            QuestionId = question.Id,
                            QuestionText = question.Text,
                            SelectedAnswer = studentAnswer?.SelectedAnswer.ToString() ?? "Not Answered",
                            CorrectAnswer = question.CorrectAnswer.ToString(),
                            IsCorrect = isCorrect,
                            Points = isCorrect ? question.Points : 0
                        });

                        // Save or update student answer
                        if (studentAnswer != null)
                        {
                            var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                            var existingAnswer = await answerSet.FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.QuestionId == question.Id);
                            
                            if (existingAnswer != null)
                            {
                                existingAnswer.Answer = studentAnswer.SelectedAnswer;
                                existingAnswer.IsCorrect = isCorrect;
                            }
                            else
                            {
                                var newAnswer = new StudentAnswerQuestion
                                {
                                    StudentId = dto.StudentId,
                                    QuestionId = question.Id,
                                    Answer = studentAnswer.SelectedAnswer,
                                    IsCorrect = isCorrect
                                };
                                await answerSet.AddAsync(newAnswer);
                            }
                        }
                    }

                    // Calculate grade and percentage
                    double percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;
                    int grade = (int)Math.Round(percentage);

                    // Update attempt status
                    attempt.EndTime = DateTime.UtcNow;
                    attempt.Grade = grade;
                    attempt.Status = QuizStatus.Completed;

                    await _unitOfWork.SaveChangesAsync();

                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDTO
                    {
                        UserId = dto.StudentId,
                        Title = "Quiz Completed",
                        Message = $"You scored {grade}% on '{quiz.Title}'. You got {correctAnswers} out of {quiz.Questions.Count} questions correct.",
                        Type = NotificationType.QuizResult
                    });
                }
                catch
                {
                    // Notification failure shouldn't fail the quiz submission
                }

                var result = new QuizResultDTO
                {
                    QuizId = quiz.Id,
                    QuizTitle = quiz.Title,
                    CourseId = quiz.CourseId,
                    StudentId = dto.StudentId,
                    TotalQuestions = quiz.Questions.Count,
                    CorrectAnswers = correctAnswers,
                    TotalPoints = totalPoints,
                    EarnedPoints = earnedPoints,
                    Percentage = percentage,
                    Grade = grade,
                    Passed = grade >= quiz.PassingScore,
                    QuestionResults = questionResults
                };

                return new ServiceResponseDTO<QuizResultDTO>
                {
                    Data = result,
                    Success = true,
                    Message = "Quiz submitted successfully."
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error submitting quiz: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetQuizzesByCourseAsync(string courseId)
        {
            try
            {
                var quizzes = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .Where(q => q.CourseId == courseId)
                    .ToListAsync();

                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Data = quizzes.Select(q => MapToReadDTO(q)).ToList(),
                    Success = true,
                    Message = "Quizzes retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving quizzes: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetQuizzesByInstructorAsync(string instructorId)
        {
            try
            {
                var quizzes = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .Where(q => q.InstructorId == instructorId)
                    .ToListAsync();

                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Data = quizzes.Select(q => MapToReadDTO(q)).ToList(),
                    Success = true,
                    Message = "Quizzes retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving quizzes: {ex.Message}"
                };
            }
        public async Task<ServiceResponseDTO<StudentQuizStatusDTO>> GetStudentQuizStatusAsync(string quizId, string studentId)
        {
            try
            {
                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                var attempt = await studentQuizSet.FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId);

                if (attempt == null)
                {
                    return new ServiceResponseDTO<StudentQuizStatusDTO>
                    {
                        Data = new StudentQuizStatusDTO
                        {
                            QuizId = quizId,
                            StudentId = studentId,
                            Status = QuizStatus.NotStarted
                        },
                        Success = true
                    };
                }

                return new ServiceResponseDTO<StudentQuizStatusDTO>
                {
                    Data = new StudentQuizStatusDTO
                    {
                        QuizId = quizId,
                        StudentId = studentId,
                        Status = attempt.Status,
                        Grade = attempt.Grade,
                        StartTime = attempt.StartTime,
                        EndTime = attempt.EndTime
                    },
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<StudentQuizStatusDTO>
                {
                    Success = false,
                    Message = $"Error retrieving quiz status: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<QuizResultDTO>> GetQuizResultAsync(string quizId, string studentId)
        {
            try
            {
                var quiz = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                    return new ServiceResponseDTO<QuizResultDTO> { Success = false, Message = "Quiz not found." };

                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                var attempt = await studentQuizSet.FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId);

                if (attempt == null || (attempt.Status != QuizStatus.Completed && attempt.Status != QuizStatus.Graded))
                {
                    return new ServiceResponseDTO<QuizResultDTO> { Success = false, Message = "Quiz not completed yet." };
                }

                var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                var questionIds = quiz.Questions.Select(q => q.Id).ToList();
                var answers = await answerSet.Where(a => a.StudentId == studentId && questionIds.Contains(a.QuestionId)).ToListAsync();

                var questionResults = new List<QuestionResultDTO>();
                int correctAnswers = 0;
                int earnedPoints = 0;
                int totalPoints = 0;

                foreach (var question in quiz.Questions)
                {
                    totalPoints += question.Points;
                    var answer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
                    bool isCorrect = answer?.IsCorrect ?? false;
                    
                    if (isCorrect)
                    {
                        correctAnswers++;
                        earnedPoints += question.Points;
                    }

                    questionResults.Add(new QuestionResultDTO
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        SelectedAnswer = answer?.Answer.ToString() ?? "Not Answered",
                        CorrectAnswer = question.CorrectAnswer.ToString(),
                        IsCorrect = isCorrect,
                        Points = isCorrect ? question.Points : 0
                    });
                }
                
                double percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;

                return new ServiceResponseDTO<QuizResultDTO>
                {
                    Data = new QuizResultDTO
                    {
                        QuizId = quiz.Id,
                        QuizTitle = quiz.Title,
                        CourseId = quiz.CourseId,
                        StudentId = studentId,
                        TotalQuestions = quiz.Questions.Count,
                        CorrectAnswers = correctAnswers,
                        TotalPoints = totalPoints,
                        EarnedPoints = earnedPoints,
                        Percentage = percentage,
                        Grade = attempt.Grade,
                        Passed = attempt.Grade >= quiz.PassingScore,
                        QuestionResults = questionResults
                    },
                    Success = true
                };
            }
            catch (Exception ex)
            {
                 return new ServiceResponseDTO<QuizResultDTO> { Success = false, Message = $"Error retrieving results: {ex.Message}" };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>> GetQuizSubmissionsAsync(string quizId)
        {
            try
            {
                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                var quiz = await GetRepo().GetQueryable()
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                    return new ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>> { Success = false, Message = "Quiz not found." };

                var submissions = await studentQuizSet
                    .Where(sq => sq.QuizId == quizId && (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .Include(sq => sq.Student)
                    .ToListAsync();

                var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                var questionSet = (DbSet<Question>)_unitOfWork.GetQueryable<Question>();
                var quizQuestions = await questionSet.Where(q => q.QuizId == quizId).ToListAsync();

                var submissionDtos = new List<QuizSubmissionDTO>();

                foreach (var submission in submissions)
                {
                    var questionIds = quizQuestions.Select(q => q.Id).ToList();
                    var studentAnswers = await answerSet
                        .Where(a => a.StudentId == submission.StudentId && questionIds.Contains(a.QuestionId))
                        .ToListAsync();

                    int correctAnswers = studentAnswers.Count(a => a.IsCorrect);

                    submissionDtos.Add(new QuizSubmissionDTO
                    {
                        StudentId = submission.StudentId,
                        StudentName = $"{submission.Student.FirstName} {submission.Student.LastName}",
                        StudentEmail = submission.Student.Email,
                        Grade = submission.Grade,
                        CompletedAt = submission.EndTime,
                        CorrectAnswers = correctAnswers,
                        TotalQuestions = quizQuestions.Count,
                        Passed = submission.Grade >= quiz.PassingScore
                    });
                }

                return new ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>
                {
                    Data = submissionDtos.OrderByDescending(s => s.CompletedAt),
                    Success = true,
                    Message = "Quiz submissions retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>> { Success = false, Message = $"Error retrieving submissions: {ex.Message}" };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetStudentQuizzesAsync(string studentId)
        {
            try
            {
                // 1. Get enrolled courses
                var studentEnrollments = await _unitOfWork.GetQueryable<StudentEnrollIntoCourse>()
                    .Where(se => se.StudentId == studentId)
                    .Select(se => se.CourseId)
                    .ToListAsync();

                // 2. Get quizzes for these courses
                var quizzes = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .Where(q => studentEnrollments.Contains(q.CourseId))
                    .OrderBy(q => q.StartDate) // 3. Sort by time
                    .ToListAsync();

                // 3. Get student attempts
                var attempts = await _unitOfWork.GetQueryable<StudentQuiz>()
                    .Where(sq => sq.StudentId == studentId)
                    .ToListAsync();

                var quizDtos = quizzes.Select(q => 
                {
                    var dto = MapToReadDTO(q);
                    var attempt = attempts.FirstOrDefault(a => a.QuizId == q.Id);
                    if (attempt != null)
                    {
                        dto.IsCompleted = attempt.Status == QuizStatus.Completed || attempt.Status == QuizStatus.Graded;
                        dto.Grade = attempt.Grade;
                    }
                    return dto;
                }).ToList();

                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Data = quizDtos,
                    Success = true,
                    Message = "Student quizzes retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving student quizzes: {ex.Message}"
                };
            }
        }
    }
}
