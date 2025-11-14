using System.ComponentModel.DataAnnotations;

public class GradeAssignmentDTO
{
    [Required]
    public string StudentAssignmentId { get; set; }

    [Required]
    [Range(0, 100)]
    public double Grade { get; set; }

    public string? Feedback { get; set; }
}