using System;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class QuizSubmissionDTO
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int? Grade { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool Passed { get; set; }
    }
}
