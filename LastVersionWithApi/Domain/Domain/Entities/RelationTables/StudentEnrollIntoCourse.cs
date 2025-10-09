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
        public string StudentId = string.Empty;
        public ApplicationUser Student;

        public string CourseId = string.Empty;
        public Course Course;
    }
}
