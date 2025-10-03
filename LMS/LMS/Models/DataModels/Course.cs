using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models.DataModels
{
    public class course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Prerequisites { get; set; }
        public string PhotoFileName { get; set; }
        public string PhotoContentType { get; set; }
        public byte[] PhotoData { get; set; }
        public int EnrolledCount { get; set; } = 0;
        public decimal Price { get; set; } = 0.00m;
        public int DurationHours { get; set; } = 0;
        [ForeignKey("Instructor")]
        public int? InstructorId { get; set; }
        public decimal AverageRating { get; set; } = 0.00m;
        public int TotalReviews { get; set; } = 0;
        public string LastUpdated { get; set; } = DateTime.Now.ToString("MMMM yyyy");
        public virtual User? Instructor { get; set; }
        // public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual List<skill> Skills { get; set; } = new List<skill>();
    }



}

