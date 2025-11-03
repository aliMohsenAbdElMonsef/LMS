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
        public IStudentEnrollIntoCourseServices StudentEnrollIntoCourse { get; }

        public UnitOfServices(IAssignmentServices assignments, ICategoryServices categories, ICertificateTemplateServices certificateTemplates, ICourseServices courses, ILectureServices lectures, IUserServices users, IStudentEnrollIntoCourseServices studentEnrollIntoCourse)
        {
            Assignments = assignments;
            Categories = categories;
            CertificateTemplates = certificateTemplates;
            Courses = courses;
            Lectures = lectures;
            //Questions = questions;
            //Quizzes = quizzes;
            //Skills = skills;
            Users = users;
            //DaySchedules = daySchedules;
            StudentEnrollIntoCourse = studentEnrollIntoCourse;
        }
    }
}
