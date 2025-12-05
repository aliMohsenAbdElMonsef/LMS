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
        private readonly IEmailService _emailService;

        public QuizServices(IUnitOfWork unitOfWork, INotificationService notificationService, IEmailService emailService) : base(unitOfWork)
        {
            _notificationService = notificationService;
            _emailService = emailService;
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
                    .Where(q => !q.IsDeleted)
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
            existingEntity.Title = dto.Title ?? existingEntity.Title;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.DurationMinutes = dto.DurationMinutes ?? existingEntity.DurationMinutes;
            existingEntity.StartDate = dto.StartDate ?? existingEntity.StartDate;
            existingEntity.EndDate = dto.EndDate ?? existingEntity.EndDate;
            existingEntity.PassingScore = dto.PassingScore ?? existingEntity.PassingScore;
            existingEntity.MaxAttempts = dto.MaxAttempts ?? existingEntity.MaxAttempts;
            existingEntity.InstructorId = dto.InstructorId ?? existingEntity.InstructorId;


            if (dto.Questions != null && dto.Questions.Any())
            {

                var dtoQuestionIds = dto.Questions
                    .Where(q => !string.IsNullOrEmpty(q.Id))
                    .Select(q => q.Id)
                    .ToHashSet();


                foreach (var existingQuestion in existingEntity.Questions.Where(q => !q.IsDeleted).ToList())
                {
                    if (!dtoQuestionIds.Contains(existingQuestion.Id))
                    {
                        existingQuestion.IsDeleted = true;
                        existingQuestion.DeletedAt = DateTime.UtcNow;
                    }
                }


                foreach (var qDto in dto.Questions)
                {
                    if (!string.IsNullOrEmpty(qDto.Id))
                    {

                        var existingQuestion = existingEntity.Questions.FirstOrDefault(q => q.Id == qDto.Id && !q.IsDeleted);
                        if (existingQuestion != null)
                        {
                            existingQuestion.Text = qDto.Text ?? existingQuestion.Text;
                            existingQuestion.OptionA = qDto.OptionA;
                            existingQuestion.OptionB = qDto.OptionB;
                            existingQuestion.OptionC = qDto.OptionC;
                            existingQuestion.OptionD = qDto.OptionD;
                            existingQuestion.CorrectAnswer = qDto.CorrectAnswer ?? existingQuestion.CorrectAnswer;
                            existingQuestion.Points = qDto.Points ?? existingQuestion.Points;
                        }
                    }
                    else
                    {

                        var newQuestionId = Guid.NewGuid().ToString();
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


                existingEntity.NumberOfQuestions = existingEntity.Questions.Count(q => !q.IsDeleted);
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


                var existingEntity = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (existingEntity == null)
                    throw new Exception($"Quiz with id '{id}' not found.");

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


                bool isStarted = quiz.StartDate == DateTime.MinValue || DateTime.Now >= quiz.StartDate;
                bool isEnded = quiz.EndDate != DateTime.MinValue && DateTime.Now > quiz.EndDate;

                if (!isStarted || isEnded)
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Quiz is not currently available." };
                }


                var isEnrolled = await _unitOfWork.StudentEnrollments.IsStudentEnrolledInCourseAsync(studentId, quiz.CourseId);
                if (!isEnrolled)
                {
                    return new ServiceResponseDTO<bool> { Success = false, Message = "You must be enrolled in the course to take this quiz." };
                }

                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();


                var completedAttempts = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId && 
                           (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .CountAsync();

                if (completedAttempts >= 1) 
                {
                    return new ServiceResponseDTO<bool> 
                    { 
                        Success = false, 
                        Message = "You have already completed this quiz. Only one attempt is allowed." 
                    };
                }

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




                bool isStarted = quiz.StartDate == DateTime.MinValue || DateTime.Now >= quiz.StartDate;
                bool isEnded = quiz.EndDate != DateTime.MinValue && DateTime.Now > quiz.EndDate;

                if (!isStarted || isEnded)
                {
                    var msg = $"Quiz is not currently available.";
                    return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = msg };
                }


                var isEnrolled = await _unitOfWork.StudentEnrollments.IsStudentEnrolledInCourseAsync(studentId, quiz.CourseId);
                if (!isEnrolled)
                {
                    return new ServiceResponseDTO<QuizAttemptDTO> { Success = false, Message = "You must be enrolled in the course to take this quiz." };
                }

                var studentQuizSet = _unitOfWork.GetQueryable<StudentQuiz>();
                

                var completedAttempts = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId && 
                           (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .CountAsync();


                if (completedAttempts >= quiz.MaxAttempts)
                {
                    return new ServiceResponseDTO<QuizAttemptDTO> 
                    { 
                        Success = false, 
                        Message = $"You have reached the maximum number of attempts ({quiz.MaxAttempts}) for this quiz." 
                    };
                }


                var attempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == studentId && sq.QuizId == quizId && sq.Status == QuizStatus.InProgress);

                int timeRemaining = quiz.DurationMinutes * 60;
                DateTime startTime = DateTime.UtcNow;


                if (attempt != null)
                {
                    startTime = attempt.StartTime;
                    var timeElapsed = DateTime.UtcNow - attempt.StartTime;
                    timeRemaining = (int)((quiz.DurationMinutes * 60) - timeElapsed.TotalSeconds);
                    
                    if (timeRemaining <= 0)
                    {

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
                        CorrectAnswer = "",
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

                var quiz = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .Include(q => q.Course)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

                if (quiz == null)
                {
                    return new ServiceResponseDTO<QuizResultDTO>
                    {
                        Message = "Quiz not found."
                    };
                }

                if (quiz.StartDate > DateTime.Now || quiz.EndDate < DateTime.Now)
                {
                    return new ServiceResponseDTO<QuizResultDTO>
                    {
                        Success = false,
                        Message = "Quiz is not currently available."
                    };
                }

                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                

                var attempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == dto.StudentId && sq.QuizId == dto.QuizId && sq.Status == QuizStatus.InProgress);

                if (attempt == null)
                {

                    var completedAttempts = await studentQuizSet
                        .Where(sq => sq.StudentId == dto.StudentId && sq.QuizId == dto.QuizId && 
                               (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                        .CountAsync();

                    if (completedAttempts >= 1)  
                    {
                        return new ServiceResponseDTO<QuizResultDTO> 
                        { 
                            Success = false, 
                            Message = "You have already submitted this quiz. Only one submission is allowed." 
                        };
                    }


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
                    int correctAnswers = 0;
                    int totalPoints = 0;
                    int earnedPoints = 0;
                    var questionResults = new List<QuestionResultDTO>();

                    foreach (var question in quiz.Questions)
                    {

                        bool isShortAnswer = string.IsNullOrEmpty(question.OptionA) && 
                                           string.IsNullOrEmpty(question.OptionB) && 
                                           string.IsNullOrEmpty(question.OptionC) && 
                                           string.IsNullOrEmpty(question.OptionD);

                        totalPoints += question.Points;
                        var studentAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                        
                        bool isCorrect = false;
                        string selectedAnswerText = "Not Answered";
                        string correctAnswerText = question.CorrectAnswer.ToString();

                        if (isShortAnswer)
                        {


                            selectedAnswerText = "Pending Review";
                            correctAnswerText = "Requires Manual Grading";

                        }
                        else
                        {

                            isCorrect = studentAnswer != null && 
                                       studentAnswer.SelectedAnswer.HasValue && 
                                       studentAnswer.SelectedAnswer.Value == question.CorrectAnswer;
                            
                            if (isCorrect)
                            {
                                correctAnswers++;
                                earnedPoints += question.Points;
                            }

                            selectedAnswerText = studentAnswer?.SelectedAnswer?.ToString() ?? "Not Answered";
                        }

                        questionResults.Add(new QuestionResultDTO
                        {
                            QuestionId = question.Id,
                            QuestionText = question.Text,
                            SelectedAnswer = selectedAnswerText,
                            CorrectAnswer = correctAnswerText,
                            IsCorrect = isCorrect,
                            Points = isCorrect ? question.Points : 0
                        });


                        if (studentAnswer != null && (studentAnswer.SelectedAnswer.HasValue || !string.IsNullOrEmpty(studentAnswer.ShortAnswerText)))
                        {
                            var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                            var existingAnswer = await answerSet.FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.QuestionId == question.Id);
                            
                            if (existingAnswer != null)
                            {
                                existingAnswer.Answer = studentAnswer.SelectedAnswer ?? Options.OptionA;
                                existingAnswer.TextAnswer = studentAnswer.ShortAnswerText;
                                existingAnswer.IsCorrect = isCorrect;
                            }
                            else
                            {
                                var newAnswer = new StudentAnswerQuestion
                                {
                                    StudentId = dto.StudentId,
                                    QuestionId = question.Id,
                                    Answer = studentAnswer.SelectedAnswer ?? Options.OptionA,
                                    TextAnswer = studentAnswer.ShortAnswerText,
                                    IsCorrect = isCorrect
                                };
                                await answerSet.AddAsync(newAnswer);
                            }
                        }
                    }


                    bool hasShortAnswerQuestions = quiz.Questions.Any(q => 
                        string.IsNullOrEmpty(q.OptionA) && 
                        string.IsNullOrEmpty(q.OptionB) && 
                        string.IsNullOrEmpty(q.OptionC) && 
                        string.IsNullOrEmpty(q.OptionD));

                    double percentage = 0;
                    int? grade = null;

                    if (hasShortAnswerQuestions)
                    {

                        attempt.Grade = null;
                        attempt.Status = QuizStatus.Completed;
                    }
                    else
                    {

                        percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;
                        grade = (int)Math.Round(percentage);
                        
                        attempt.Grade = grade;
                        attempt.Status = QuizStatus.Graded;
                    }

                    attempt.EndTime = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync();

                try
                {
                    string notificationMessage;
                    string emailSubject;
                    string emailBody;
                    
                    if (hasShortAnswerQuestions)
                    {
                        notificationMessage = $"You have submitted '{quiz.Title}'. Your quiz is pending manual review by the instructor.";
                        emailSubject = "Quiz Submitted - Pending Review";
                        emailBody = $"<h2>Quiz Submitted</h2><p>You have successfully submitted the quiz '<strong>{quiz.Title}</strong>' for the course '<strong>{quiz.Course.Name}</strong>'.</p><p>Your quiz contains short-answer questions and is pending manual review by the instructor. You will receive another email once your quiz has been graded.</p>";
                    }
                    else
                    {
                        notificationMessage = $"You scored {attempt.Grade}% on '{quiz.Title}'. You got {correctAnswers} out of {quiz.Questions.Count} questions correct.";
                        emailSubject = "Quiz Graded";
                        emailBody = $"<h2>Quiz Graded</h2><p>Your quiz '<strong>{quiz.Title}</strong>' for the course '<strong>{quiz.Course.Name}</strong>' has been graded!</p><h3>Results:</h3><ul><li>Score: <strong>{attempt.Grade}%</strong></li><li>Correct Answers: <strong>{correctAnswers} out of {quiz.Questions.Count}</strong></li><li>Status: <strong>{(attempt.Grade >= quiz.PassingScore ? "Passed" : "Failed")}</strong></li></ul>";
                    }

                    await _notificationService.CreateNotificationAsync(new CreateNotificationDTO
                    {
                        UserId = dto.StudentId,
                        Title = "Quiz Completed",
                        Message = notificationMessage,
                        Type = NotificationType.QuizResult
                    });

                    // Send email notification
                    var student = await _unitOfWork.Users.FindByIdAsync(dto.StudentId);
                    if (student != null && !string.IsNullOrEmpty(student.Email))
                    {
                        await _emailService.SendEmailAsync(student.Email, emailSubject, emailBody);
                    }
                }
                catch
                {

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
                    Passed = grade.HasValue && grade.Value >= quiz.PassingScore,
                    IsPendingGrading = grade == null,
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


                var instructorEnrollments = await _unitOfWork.InstructorEnrollments.GetAllAsync();
                var activeCourseIds = instructorEnrollments
                    .Where(e => e.InstructorId == instructorId && !e.IsDeleted && e.Status == Domain.Enums.ApplicationStatus.Approved)
                    .Select(e => e.CourseId)
                    .ToHashSet();
                
                var filteredQuizzes = quizzes.Where(q => activeCourseIds.Contains(q.CourseId)).ToList();

                return new ServiceResponseDTO<IEnumerable<ReadQuizDTO>>
                {
                    Data = filteredQuizzes.Select(q => MapToReadDTO(q)).ToList(),
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

        public async Task<ServiceResponseDTO<StudentQuizStatusDTO>> GetStudentQuizStatusAsync(string quizId, string studentId)
        {
            try
            {
                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                var attempts = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId)
                    .ToListAsync();
                

                var bestAttempt = attempts
                    .Where(a => a.Status == QuizStatus.Completed || a.Status == QuizStatus.Graded)
                    .OrderByDescending(a => a.Grade)
                    .FirstOrDefault();


                var attempt = bestAttempt ?? attempts.OrderByDescending(a => a.StartTime).FirstOrDefault();

                if (attempt == null)
                {
                    return new ServiceResponseDTO<StudentQuizStatusDTO>
                    {
                        Success = true,
                        Data = new StudentQuizStatusDTO { Status = QuizStatus.NotStarted }
                    };
                }

                return new ServiceResponseDTO<StudentQuizStatusDTO>
                {
                    Success = true,
                    Data = new StudentQuizStatusDTO 
                    { 
                        Status = attempt.Status,
                        Grade = attempt.Grade,
                        StartTime = attempt.StartTime,
                        EndTime = attempt.EndTime
                    }
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

        public async Task<ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>> GetQuizSubmissionsAsync(string quizId)
        {
            try
            {
                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                
                var submissions = await studentQuizSet
                    .Include(sq => sq.Student)
                    .Where(sq => sq.QuizId == quizId && (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .OrderByDescending(sq => sq.EndTime)
                    .ToListAsync();

                var dtos = submissions.Select(s => new QuizSubmissionDTO
                {
                    StudentId = s.StudentId,
                    StudentName = s.Student != null ? $"{s.Student.FirstName} {s.Student.LastName}" : "Unknown Student",
                    StudentEmail = s.Student?.Email ?? "",
                    CompletedAt = s.EndTime ?? DateTime.MinValue,
                    Grade = s.Grade,
                    Passed = s.Grade.HasValue && s.Grade.Value >= 60
                }).ToList();

                return new ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>
                {
                    Success = true,
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<IEnumerable<QuizSubmissionDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving submissions: {ex.Message}"
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
                var attempt = await studentQuizSet
                    .Where(sq => sq.StudentId == studentId && sq.QuizId == quizId && 
                           (sq.Status == QuizStatus.Completed || sq.Status == QuizStatus.Graded))
                    .OrderByDescending(sq => sq.Grade)
                    .FirstOrDefaultAsync();

                if (attempt == null)
                    return new ServiceResponseDTO<QuizResultDTO> { Success = false, Message = "Quiz result not found." };


                var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                var answers = await answerSet
                    .Where(a => a.StudentId == studentId && quiz.Questions.Select(q => q.Id).Contains(a.QuestionId))
                    .ToListAsync();

                var questionResults = new List<QuestionResultDTO>();
                int correctAnswers = 0;
                int totalPoints = 0;
                int earnedPoints = 0;

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

                    string selectedAnswerText = "Not Answered";
                    if (answer != null)
                    {
                        if (!string.IsNullOrEmpty(answer.TextAnswer))
                            selectedAnswerText = answer.TextAnswer;
                        else
                            selectedAnswerText = answer.Answer.ToString();
                    }


                    bool isShortAnswer = string.IsNullOrEmpty(question.OptionA) && 
                                       string.IsNullOrEmpty(question.OptionB) && 
                                       string.IsNullOrEmpty(question.OptionC) && 
                                       string.IsNullOrEmpty(question.OptionD);

                    string correctAnswerText = question.CorrectAnswer.ToString();
                    if (isShortAnswer)
                    {
                        correctAnswerText = "Requires Manual Grading";

                        if (attempt.Status == QuizStatus.Graded)
                        {

                        }
                        else
                        {
                            selectedAnswerText += " (Pending Review)";
                        }
                    }

                    questionResults.Add(new QuestionResultDTO
                    {
                        QuestionId = question.Id,
                        QuestionText = question.Text,
                        SelectedAnswer = selectedAnswerText,
                        CorrectAnswer = correctAnswerText,
                        IsCorrect = isCorrect,
                        Points = isCorrect ? question.Points : 0
                    });
                }

                var result = new QuizResultDTO
                {
                    QuizId = quiz.Id,
                    QuizTitle = quiz.Title,
                    CourseId = quiz.CourseId,
                    StudentId = studentId,
                    TotalQuestions = quiz.Questions.Count,
                    CorrectAnswers = correctAnswers,
                    TotalPoints = totalPoints,
                    EarnedPoints = earnedPoints,
                    Percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0,
                    Grade = attempt.Grade,
                    Passed = attempt.Grade.HasValue && attempt.Grade.Value >= quiz.PassingScore,
                    IsPendingGrading = attempt.Status == QuizStatus.Completed,
                    QuestionResults = questionResults
                };

                return new ServiceResponseDTO<QuizResultDTO>
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<QuizResultDTO>
                {
                    Success = false,
                    Message = $"Error retrieving results: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<bool>> GradeQuizAsync(ManualGradeDTO dto)
        {
            try
            {
                var studentQuizSet = (DbSet<StudentQuiz>)_unitOfWork.GetQueryable<StudentQuiz>();
                var attempt = await studentQuizSet
                    .FirstOrDefaultAsync(sq => sq.StudentId == dto.StudentId && sq.QuizId == dto.QuizId);

                if (attempt == null)
                    return new ServiceResponseDTO<bool> { Success = false, Message = "Quiz attempt not found." };

                var answerSet = (DbSet<StudentAnswerQuestion>)_unitOfWork.GetQueryable<StudentAnswerQuestion>();
                var answers = await answerSet
                    .Where(a => a.StudentId == dto.StudentId && dto.Grades.Select(g => g.QuestionId).Contains(a.QuestionId))
                    .ToListAsync();


                foreach (var grade in dto.Grades)
                {
                    var answer = answers.FirstOrDefault(a => a.QuestionId == grade.QuestionId);
                    if (answer != null)
                    {
                        answer.IsCorrect = grade.IsCorrect;
                    }
                }


                var quiz = await GetRepo().GetQueryable()
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

                if (quiz != null)
                {

                    var allAnswers = await answerSet
                        .Where(a => a.StudentId == dto.StudentId && quiz.Questions.Select(q => q.Id).Contains(a.QuestionId))
                        .ToListAsync();

                    int totalPoints = quiz.Questions.Sum(q => q.Points);
                    int earnedPoints = 0;

                    foreach (var question in quiz.Questions)
                    {
                        var answer = allAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                        if (answer != null && answer.IsCorrect)
                        {
                            earnedPoints += question.Points;
                        }
                    }

                    double percentage = totalPoints > 0 ? (double)earnedPoints / totalPoints * 100 : 0;
                    attempt.Grade = (int)Math.Round(percentage);
                    attempt.Status = QuizStatus.Graded;
                }

                await _unitOfWork.SaveChangesAsync();


                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDTO
                    {
                        UserId = dto.StudentId,
                        Title = "Quiz Graded",
                        Message = $"Your quiz '{quiz?.Title}' has been graded. You scored {attempt.Grade}%.",
                        Type = NotificationType.QuizResult
                    });
                    var student = await _unitOfWork.Users.FindByIdAsync(dto.StudentId);
                    if (student != null && !string.IsNullOrEmpty(student.Email) && quiz != null)
                    {
                        var emailSubject = "Quiz Graded";
                        var emailBody = $"<h2>Quiz Graded</h2><p>Your quiz '<strong>{quiz.Title}</strong>' for the course '<strong>{quiz.Course?.Name}</strong>' has been graded by your instructor!</p><h3>Results:</h3><ul><li>Score: <strong>{attempt.Grade}%</strong></li><li>Status: <strong>{(attempt.Grade >= quiz.PassingScore ? "Passed" : "Failed")}</strong></li></ul><p>You can view your detailed results in the LMS.</p>";
                        await _emailService.SendEmailAsync(student.Email, emailSubject, emailBody);
                    }

                }
                catch { }

                return new ServiceResponseDTO<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Quiz graded successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseDTO<bool>
                {
                    Success = false,
                    Message = $"Error grading quiz: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadQuizDTO>>> GetStudentQuizzesAsync(string studentId)
        {
            try
            {

                var enrollments = await _unitOfWork.StudentEnrollments.GetAllAsync(e => e.StudentId == studentId);
                var courseIds = enrollments.Select(e => e.CourseId).ToList();
                
                var quizzes = await GetRepo().GetQueryable()
                    .Include(q => q.Course)
                    .Include(q => q.Instructor)
                    .Include(q => q.Questions)
                    .Where(q => courseIds.Contains(q.CourseId))
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
        


    }
}
