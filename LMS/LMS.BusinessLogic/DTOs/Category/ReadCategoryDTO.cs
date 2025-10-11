using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Category
{
    public class ReadCategoryDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string AdminId { get; set; }
        public string? AdminName { get; set; }

        public int CoursesCount { get; set; }
    }
}
