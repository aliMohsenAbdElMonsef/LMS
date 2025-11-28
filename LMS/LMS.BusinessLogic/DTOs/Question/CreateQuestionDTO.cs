using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Question
{
    // check if the quiz pass or not
    public class CreateQuestionDTO
    {
        [Required]
        public string QuizId { get; set; }
        [Required]
        public string Text { get; set; }

        public string? OptionA { get; set; }

        public string? OptionB { get; set; }

        public string? OptionC { get; set; }

        public string? OptionD { get; set; }

        public Options CorrectAnswer { get; set; } 

        public int Points { get; set; } = 1;
    }
}
