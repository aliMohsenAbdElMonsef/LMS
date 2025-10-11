using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class Assignment : SoftDeletion
    {
        public string Id { get; set; }
        public Assignment()
        {
            Id = Guid.NewGuid().ToString();
        }
        [MaxLength(150)]
        public string Title { get; set; }
        
        public string? Description { get; set; }

        [MaxLength(500)]
        public string FilePath {  get; set; } 
        public DateTime UploadDate { get; set; }= DateTime.Now;
        public DateTime DueDate { get; set; }

        // Relations
        // user
            // instructor
        [Required]
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }

           // student
         public ICollection<StudentAssignment> Students { get; set; } = new List<StudentAssignment>();
        // course

        [Required]
        public string CourseId { get; set; }
        public Course Course { get; set; }


    }
}
