using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentQuiz
    {
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; }
        public string QuizId { get; set; } = string.Empty;
        public Quiz Quiz { get; set; }

        public int? Grade { get; set; }
    }
}
