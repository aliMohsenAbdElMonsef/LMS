using Domain.Entities.RelationTables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MainEntities
{
    public class Skills : SoftDeletion
    {
        public string Id { get; set; }
        public Skills()
        {
            Id = Guid.NewGuid().ToString();
        }
        [MaxLength(100)]
        public string Name { get; set; }
        
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;
        // relations
        // admin
        [Required]
        public string AdminId { get; set; }

        public ApplicationUser Admin { get; set; }

        // course
        public ICollection<CourseSkill> Courses { get; set; } = new List<CourseSkill>();

    }
}
