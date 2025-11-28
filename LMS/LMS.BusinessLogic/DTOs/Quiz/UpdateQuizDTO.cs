using LMS.BusinessLogic.DTOs.Question;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Quiz
{
    public class UpdateQuizDTO
    {
        [Required]
        public string Id { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? DurationMinutes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, 100)]
        public int? PassingScore { get; set; }

        public string? InstructorId { get; set; }

        public ICollection<UpdateQuestionDTO> Questions { get; set; } = new List<UpdateQuestionDTO>();
    }
}
