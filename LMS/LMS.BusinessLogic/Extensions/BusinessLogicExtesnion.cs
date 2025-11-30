using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace LMS.BusinessLogic.Extensions
{
    public static class BusinessLogicExtension
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            services.AddScoped<ITokenServices, TokenServices>();
            services.AddScoped<IBlackListedTokensServices, BlackListedTokensServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryServices, CategoryServices>();
            services.AddScoped<ICourseServices, CourseServices>();
            services.AddScoped<IAssignmentServices, AssignmentServices>();
            services.AddScoped<ILectureServices, LectureService>();
            services.AddScoped<IStudentEnrollment, StudentEnrollIntoCourseService>();
            services.AddScoped<IInstructorEnrollIntoCourse, InstructorEnrollIntoCourseService>();
            services.AddScoped<ICertificateTemplateServices, CertificateTemplateServices>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            // Add this to your service registration
            services.AddScoped<IEnrollmentManagement, EnrollmentManagementService>();
            services.AddScoped<ILectureScheduleService, LectureScheduleService>();

            //services.AddScoped<IQuestionServices, QuestionServices>();
            services.AddScoped<IQuizServices, QuizServices>();
            services.AddScoped<ILectureReminderService, LectureReminderService>();
            //services.AddScoped<ISkillServices, SkillServices>();
            //services.AddScoped<ICourseDayScheduleServices, CourseDayScheduleServices>();
            services.AddScoped<IUnitOfServices, UnitOfServices>();

            // Register EmailSettings
            var emailSettings = configuration.GetSection("EmailSettings").Get<LMS.BusinessLogic.DTOs.Email.EmailSettings>();
            if (emailSettings == null)
            {
                // Fallback or throw, but for now let's create a default to prevent startup crash if config is missing
                emailSettings = new LMS.BusinessLogic.DTOs.Email.EmailSettings { UseMockEmail = true };
            }
            services.AddSingleton(emailSettings);

            return services;
        }
    }

}
