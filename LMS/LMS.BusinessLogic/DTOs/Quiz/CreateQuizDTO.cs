using LMS.BusinessLogic.DTOs.Question;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class CreateQuizDTO
    {
        [Required, MaxLength(150)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0, 100)]
        public int PassingScore { get; set; }

        [Required]
        public string CourseId { get; set; }

        [Required]
        public string InstructorId { get; set; }

        public ICollection<CreateQuestionDTO> Questions { get; set; } = new List<CreateQuestionDTO>();
    }
}
