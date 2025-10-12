using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SkillDTOs
{
    public class CreateSkillDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string AdminId { get; set; } = string.Empty;
    }
}
