using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class Quiz : SoftDeletion
    {
        public string Id { get; set; }
        public Quiz()
        {
            Id = Guid.NewGuid().ToString();
        }
        [MaxLength(150)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int NumberOfQuestions { get; set; }
        
        public int PassingScore { get; set; }  // Minimum score percentage required to pass
        
        public int MaxAttempts { get; set; } = 1;  // Maximum number of attempts allowed per student
        
        // relations
        // question
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        //course
        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }
        // user
        // instructor
        [Required]
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }

        // student
         public ICollection<StudentQuiz> Students { get; set; } = new List<StudentQuiz>();

    }
}
