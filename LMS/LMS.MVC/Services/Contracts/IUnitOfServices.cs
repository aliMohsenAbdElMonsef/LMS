using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Contracts
{
    public interface IUnitOfServices
    {
        IAccountService AccountService { get; }
        IAssignmentService AssignmentService { get; }
        IUserService UserService { get; }
        ICourseService CourseService { get; }

        ICategoryService CategoryService { get; }
        IEnrollmentService EnrollmentService {  get; }
        ILectureService LectureService { get; }
        IProfileService ProfileService { get; }
        IQuizService QuizService { get; }

        IHomeMVCService HomeService { get; }
    }
}
