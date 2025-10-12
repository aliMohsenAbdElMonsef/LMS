using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SkillDTOs
{
    public class UpdateSkillDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}
