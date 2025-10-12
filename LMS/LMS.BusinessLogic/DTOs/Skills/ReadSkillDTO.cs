using System;
using System.Collections.Generic;

namespace Application.DTOs.SkillDTOs
{
    public class ReadSkillDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string AdminId { get; set; } = string.Empty;
        public string? AdminFullName { get; set; }

    }

    
}
