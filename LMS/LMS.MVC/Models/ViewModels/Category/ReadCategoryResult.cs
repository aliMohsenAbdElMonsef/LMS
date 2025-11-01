using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Category
{
    public class ReadCategoryResult
    {
        public string? Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        public string? AdminId { get; set; }
        public string? AdminName { get; set; }

        public int CoursesCount { get; set; } = 0;
    }
}
