using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Category
{
    public class UpdateCategoryDTO
    {
        public string Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }
        public string AdminID { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
    }
}
