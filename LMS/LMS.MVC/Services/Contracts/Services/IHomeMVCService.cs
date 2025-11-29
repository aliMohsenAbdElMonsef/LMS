using LMS.MVC.Models.ViewModels.Category;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IHomeMVCService
    {
        Task<List<ReadCategoryResult>> GetTopCategoriesAsync(int count = 6);
        Task<List<ReadCourseResult>> GetPopularCoursesAsync(int count = 6);
    }
}