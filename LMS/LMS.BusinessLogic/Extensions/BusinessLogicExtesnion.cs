using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;


namespace LMS.BusinessLogic.Extensions
{
    public static class BusinessLogicExtension
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenServices, TokenServices>();
            services.AddScoped<IBlackListedTokensServices, BlackListedTokensServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryServices, CategoryServices>();
            services.AddScoped<ICourseServices, CourseServices>();
            services.AddScoped<IAssignmentServices, AssignmentServices>();
            services.AddScoped<ILectureServices, LectureService>();
            services.AddScoped<IStudentEnrollIntoCourseServices, StudentEnrollIntoCourseServices>();
            services.AddScoped<ICertificateTemplateServices, CertificateTemplateServices>();
            services.AddScoped<IFileService, FileService>();

            //services.AddScoped<IQuestionServices, QuestionServices>();
            //services.AddScoped<IQuizServices, QuizServices>();
            //services.AddScoped<ISkillServices, SkillServices>();
            //services.AddScoped<ICourseDayScheduleServices, CourseDayScheduleServices>();
            services.AddScoped<IUnitOfServices, UnitOfServices>();

            return services;
        }
    }

}
