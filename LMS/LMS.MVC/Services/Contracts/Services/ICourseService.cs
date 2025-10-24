using LMS.BusinessLogic.DTOs.Course;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<ReadCourseDTO>> GetAllCoursesAsync();
        Task<ReadCourseDTO?> GetCourseByIdAsync(string id);
    }
}
