using LMS.BusinessLogic.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class ReadQuizDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfQuestions { get; set; }
        public int PassingScore { get; set; }

        public string CourseId { get; set; }
        public string CourseName { get; set; }

        public string InstructorId { get; set; }
        public string InstructorName { get; set; }

        public ICollection<ReadQuestionDTO> Questions { get; set; } = new List<ReadQuestionDTO>();
    }
}
