using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;


namespace LMS.BusinessLogic.Services
{
    internal class UnitOfServices : IUnitOfServices
    {
        public IAssignmentServices Assignments { get; }
        public ICategoryServices Categories { get; }
        public ICertificateTemplateServices CertificateTemplates { get; }
        public ICourseServices Courses { get; }
        public ILectureServices Lectures { get; }
        public IQuestionServices Questions { get; }
        public IQuizServices Quizzes { get; }
        public ISkillServices Skills { get; }
        public IUserServices Users { get; }
        public ICourseDayScheduleServices DaySchedules { get; }
        public IStudentEnrollment StudentEnrollIntoCourse { get; }
        public IInstructorEnrollIntoCourse InstructorEnrollIntoCourse { get; }
        public IEnrollmentManagement EnrollmentManagement { get; }
        public IFileService FileService { get; }

        public UnitOfServices(IAssignmentServices assignments, IFileService fileSerivce, IEnrollmentManagement enrollmentManagement,ICategoryServices categories, ICertificateTemplateServices certificateTemplates, ICourseServices courses, ILectureServices lectures, IUserServices users, IStudentEnrollment studentEnrollIntoCourse, IInstructorEnrollIntoCourse instructorEnrollIntoCourse, IQuizServices quizzes)
        {
            Assignments = assignments;
            Categories = categories;
            CertificateTemplates = certificateTemplates;
            Courses = courses;
            Lectures = lectures;
            FileService = fileSerivce;
            Users = users;
            StudentEnrollIntoCourse = studentEnrollIntoCourse;
            InstructorEnrollIntoCourse = instructorEnrollIntoCourse;
            EnrollmentManagement = enrollmentManagement;
            Quizzes = quizzes;
        }
    }
}
