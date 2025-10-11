using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class StudentEnrollIntoCourse
    {
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student;

        public string CourseId { get; set; } = string.Empty;
        public Course Course;
    }
}
