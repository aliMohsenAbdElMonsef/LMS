using LMS.BusinessLogic.DTOs.Course;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<AllCoursesResult>> GetAllCoursesAsync();
        //Task<ReadCourseDTO?> GetCourseByIdAsync(string id);
    }
}
