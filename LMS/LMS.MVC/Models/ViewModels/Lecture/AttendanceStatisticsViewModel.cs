namespace LMS.MVC.Models.ViewModels.Lecture
{
    public class AttendanceStatisticsViewModel
    {
        public int TotalLectures { get; set; }
        public int AttendedLectures { get; set; }
        public double AttendancePercentage { get; set; }
    }
}
