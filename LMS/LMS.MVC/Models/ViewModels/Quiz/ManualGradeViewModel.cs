using System.Collections.Generic;

namespace LMS.MVC.Models.ViewModels.Quiz
{
    public class ManualGradeViewModel
    {
        public string QuizId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string QuizTitle { get; set; } = string.Empty;
        public List<QuestionGradeViewModel> Questions { get; set; } = new List<QuestionGradeViewModel>();
    }

    public class QuestionGradeViewModel
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string StudentAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
