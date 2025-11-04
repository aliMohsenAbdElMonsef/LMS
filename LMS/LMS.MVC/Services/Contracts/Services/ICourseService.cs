using LMS.MVC.Models.ViewModels.Course;
using LMS.MVC.Services.Response;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface ICourseService
    {
        Task<SuccessServiceResult<IEnumerable<ReadCourseResult>>> GetAllCoursesAsync();

        Task<SuccessServiceResult<ReadCourseResult>> CreateCourse(CreateCourseViewModel vm);

        Task<SuccessServiceResult<EditCourseViewModel>> GetCourseForEdit(Guid id);

        Task<SuccessServiceResult<ReadCourseResult>> UpdateCourse(Guid id, EditCourseViewModel model);

        Task<bool> DeleteCourse(Guid id);
        Task<SuccessServiceResult<ReadCourseViewModel>> GetCourseDetails(Guid id);
    }
}
