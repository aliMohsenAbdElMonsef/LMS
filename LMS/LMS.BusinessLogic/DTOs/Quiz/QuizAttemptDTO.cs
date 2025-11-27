using LMS.BusinessLogic.DTOs.Question;
using System;
using System.Collections.Generic;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class QuizAttemptDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TimeRemainingSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public List<ReadQuestionDTO> Questions { get; set; } = new List<ReadQuestionDTO>();
    }
}
