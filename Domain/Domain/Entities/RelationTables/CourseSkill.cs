using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.RelationTables
{
    public class CourseSkill
    {
        public string CourseId { get; set; }= string.Empty;
        public Course Course { get; set; }
        public string SkillId { get; set; } = string.Empty;
        public Skills Skill { get; set; }
    }
}
