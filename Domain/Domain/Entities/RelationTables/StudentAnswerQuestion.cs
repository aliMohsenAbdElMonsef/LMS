using Domain.Entities.MainEntities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentAnswerQuestion
    {

        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; }
        public string QuestionId { get; set; } = string.Empty;
        public Question Question { get; set; }
        public Options Answer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
