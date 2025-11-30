using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class SubmitQuizDTO
    {
        [Required]
        public string QuizId { get; set; }

        [Required]
        public string StudentId { get; set; }

        public List<StudentAnswerDTO> Answers { get; set; } = new List<StudentAnswerDTO>();
    }

    public class StudentAnswerDTO
    {
        [Required]
        public string QuestionId { get; set; }

        // Nullable to allow unanswered questions (null instead of defaulting to OptionA)
        public Options? SelectedAnswer { get; set; }
        public string? ShortAnswerText { get; set; }
    }

    public class QuizResultDTO
    {
        public string QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string CourseId { get; set; }
        public string StudentId { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalPoints { get; set; }
        public int EarnedPoints { get; set; }
        public double Percentage { get; set; }
        public int? Grade { get; set; }
        public bool Passed { get; set; }
        public bool IsPendingGrading { get; set; }
        public List<QuestionResultDTO> QuestionResults { get; set; } = new List<QuestionResultDTO>();
    }

    public class QuestionResultDTO
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int Points { get; set; }
    }
}
