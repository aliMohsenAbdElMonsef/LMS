using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class ManualGradeDTO
    {
        [Required]
        public string QuizId { get; set; }

        [Required]
        public string StudentId { get; set; }

        public List<QuestionGradeDTO> Grades { get; set; } = new List<QuestionGradeDTO>();
    }

    public class QuestionGradeDTO
    {
        [Required]
        public string QuestionId { get; set; }

        public bool IsCorrect { get; set; }
    }
}
