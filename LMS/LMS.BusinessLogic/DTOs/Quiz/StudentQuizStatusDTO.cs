using Domain.Enums;
using System;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class StudentQuizStatusDTO
    {
        public string QuizId { get; set; }
        public string StudentId { get; set; }
        public QuizStatus Status { get; set; }
        public int? Grade { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
