using Domain.Enums;
using LMS.BusinessLogic.DTOs.DaySchedule;
using System.ComponentModel.DataAnnotations;

namespace LMS.MVC.Models.ViewModels.Course
{
    public class CreateCourseViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Course name is required")]
        [MaxLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
        [Display(Name = "Course Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery mode is required")]
        [Display(Name = "Delivery Mode")]
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Online;

        [Required(ErrorMessage = "Credits are required")]
        [Range(0, 300, ErrorMessage = "Credits must be between 0 and 300")]
        [Display(Name = "Credits")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Language is required")]
        [Display(Name = "Language")]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7 + 84); 

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10,000")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Free Course")]
        public bool IsFree { get; set; } = false;

        [Display(Name = "Course Thumbnail")]
        public IFormFile? ThumbnailFile { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string CategoryId { get; set; } = string.Empty;

        [Display(Name = "Certificate Template")]
        public string? CertificateTemplateId { get; set; }

        [Range(0, 100, ErrorMessage = "Attendance percentage must be between 0 and 100")]
        [Display(Name = "Minimum Attendance %")]
        public double MinAttendancePercentage { get; set; } = 75;

        [Range(0, 100, ErrorMessage = "Performance score must be between 0 and 100")]
        [Display(Name = "Minimum Performance Score")]
        public double MinPerformanceScore { get; set; } = 60;

        [Display(Name = "Auto-Issue Certificates")]
        public bool AutoIssueCertificates { get; set; } = false;

        [Required(ErrorMessage = "Days per week is required")]
        [Range(1, 7, ErrorMessage = "Days per week must be between 1 and 7")]
        [Display(Name = "Days Per Week")]
        public int DaysPerWeek { get; set; } = 3;

        [Required(ErrorMessage = "Hours per session is required")]
        [Range(0.5, 12, ErrorMessage = "Hours per session must be between 0.5 and 12")]
        [Display(Name = "Hours Per Session")]
        public double HoursPerSession { get; set; } = 2;

        [Display(Name = "Generate Schedule Automatically")]
        public bool GenerateScheduleAutomatically { get; set; } = true;

        [Required(ErrorMessage = "Default start time is required")]
        [Display(Name = "Default Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan DefaultStartTime { get; set; } = new TimeSpan(9, 0, 0); 

        [Required(ErrorMessage = "Default end time is required")]
        [Display(Name = "Default End Time")]
        [DataType(DataType.Time)]
        public TimeSpan DefaultEndTime { get; set; } = new TimeSpan(11, 0, 0); 

        public List<CreateDayScheduleDTO> DaySchedules { get; set; } = new List<CreateDayScheduleDTO>();

        [Required(ErrorMessage = "Please select days of the week")]
        [MinLength(1, ErrorMessage = "Please select at least one day")]
        [Display(Name = "Selected Days")]
        public List<int> SelectedDays { get; set; } = new List<int>();

        [Display(Name = "Total Sessions")]
        public int TotalSessions => CalculateTotalSessions();

        [Display(Name = "Total Weeks")]
        public int TotalWeeks => CalculateTotalWeeks();

        [Display(Name = "Total Hours")]
        public double TotalHours => TotalSessions * HoursPerSession;

        public List<DayOfWeekOption> AvailableDays { get; } = new List<DayOfWeekOption>
        {
            new DayOfWeekOption { Value = 0, Name = "Sunday" },
            new DayOfWeekOption { Value = 1, Name = "Monday" },
            new DayOfWeekOption { Value = 2, Name = "Tuesday" },
            new DayOfWeekOption { Value = 3, Name = "Wednesday" },
            new DayOfWeekOption { Value = 4, Name = "Thursday" },
            new DayOfWeekOption { Value = 5, Name = "Friday" },
            new DayOfWeekOption { Value = 6, Name = "Saturday" }
        };

        private int CalculateTotalWeeks()
        {
            if (StartDate == default || EndDate == default)
                return 0;

            var totalDays = (EndDate - StartDate).Days;
            return (int)Math.Ceiling(totalDays / 7.0);
        }

        private int CalculateTotalSessions()
        {
            return TotalWeeks * DaysPerWeek;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after start date.",
                    new[] { nameof(EndDate) });
            }

            if (StartDate < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Start date cannot be in the past.",
                    new[] { nameof(StartDate) });
            }

            if (DefaultEndTime <= DefaultStartTime)
            {
                yield return new ValidationResult(
                    "End time must be after start time.",
                    new[] { nameof(DefaultEndTime) });
            }

            var sessionDuration = DefaultEndTime - DefaultStartTime;
            if (sessionDuration.TotalHours < HoursPerSession)
            {
                yield return new ValidationResult(
                    $"Session duration ({sessionDuration.TotalHours}h) is less than specified hours per session ({HoursPerSession}h).",
                    new[] { nameof(DefaultEndTime) });
            }

            if (SelectedDays.Count != DaysPerWeek)
            {
                yield return new ValidationResult(
                    $"Number of selected days ({SelectedDays.Count}) must match days per week ({DaysPerWeek}).",
                    new[] { nameof(SelectedDays) });
            }

            if (SelectedDays.Distinct().Count() != SelectedDays.Count)
            {
                yield return new ValidationResult(
                    "Duplicate days are not allowed.",
                    new[] { nameof(SelectedDays) });
            }

            if (TotalWeeks < 1)
            {
                yield return new ValidationResult(
                    "Course duration must be at least 1 week.",
                    new[] { nameof(EndDate) });
            }

            if (TotalWeeks > 52)
            {
                yield return new ValidationResult(
                    "Course duration cannot exceed 52 weeks.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}