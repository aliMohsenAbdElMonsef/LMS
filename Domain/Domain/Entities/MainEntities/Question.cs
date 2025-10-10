using Domain.Entities.RelationTables;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class Question : SoftDeletion
    {
        public string Id { get; set; }
        public Question()
        {
            Id = Guid.NewGuid().ToString();
        }
       
        public string Text { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }

        public Options CorrectAnswer { get; set; }

        public int Points { get; set; } = 1;

        // relations
        // quiz
        [Required]
        public string QuizId { get; set; }
        public Quiz Quiz { get; set; }
        // user
        // student
        public ICollection<StudentAnswerQuestion> StudentAnswers { get; set; } = new List<StudentAnswerQuestion>();


    }
}
