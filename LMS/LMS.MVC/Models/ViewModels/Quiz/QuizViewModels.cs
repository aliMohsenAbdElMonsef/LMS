namespace LMS.MVC.Models.ViewModels.Quiz
{
    public class QuizItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfQuestions { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public int MaxAttempts { get; set; }
        public bool IsCompleted { get; set; }
        public int AchievedScore { get; set; }
        public bool IsLocked { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<LMS.BusinessLogic.DTOs.Question.ReadQuestionDTO>? Questions { get; set; }
    }

    public class QuizListViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public IEnumerable<QuizItemViewModel> Quizzes { get; set; } = new List<QuizItemViewModel>();
    }

    public class CreateQuizViewModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfQuestions { get; set; }
        public int DurationMinutes { get; set; }
        public int PassingScore { get; set; }
        public int MaxAttempts { get; set; } = 1;
        public string? InstructorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CreateQuestionViewModel> Questions { get; set; } = new List<CreateQuestionViewModel>();
    }

    public class CreateQuestionViewModel
    {
        public string? Id { get; set; }  // For updates
        public string Text { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public Domain.Enums.Options CorrectAnswer { get; set; }
        public int Points { get; set; } = 1;
        public string Type { get; set; } = "MultipleChoice";
        public string? QuizId { get; set; }
    }

    public class UpdateQuizViewModel : CreateQuizViewModel
    {
        public string Id { get; set; } = string.Empty;
    }

    public class TakeQuizViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TimeRemainingSeconds { get; set; }
        public List<LMS.BusinessLogic.DTOs.Question.ReadQuestionDTO> Questions { get; set; } = new List<LMS.BusinessLogic.DTOs.Question.ReadQuestionDTO>();
        public List<LMS.BusinessLogic.DTOs.Quiz.SubmitQuizAnswerDTO> Answers { get; set; } = new List<LMS.BusinessLogic.DTOs.Quiz.SubmitQuizAnswerDTO>();
    }

    public class SubmitQuizViewModel
    {
        public string QuizId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public List<LMS.BusinessLogic.DTOs.Quiz.SubmitQuizAnswerDTO> Answers { get; set; } = new List<LMS.BusinessLogic.DTOs.Quiz.SubmitQuizAnswerDTO>();
    }

    public class QuizResultViewModel
    {
        public string QuizId { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public int Grade { get; set; }
        public bool Passed { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }
        public IEnumerable<QuestionResultViewModel> QuestionResults { get; set; } = new List<QuestionResultViewModel>();
    }

    public class QuestionResultViewModel
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Points { get; set; }
    }
}
