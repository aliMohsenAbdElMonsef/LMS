using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels
{
    public class CreateSkillViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

    }
}

