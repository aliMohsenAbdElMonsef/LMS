using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;


namespace LMS.BusinessLogic.Extensions
{
    public static class BusinessLogicExtesnion
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenServices, TokenServices>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryServices, CategoryServices>();
            services.AddScoped<ICourseServices, CourseServices>();
            services.AddScoped<IAssignmentServices, AssignmentServices>();
            services.AddScoped<ILectureServices, LectureService>();
            services.AddScoped<IUnitOfServices, UnitOfServices>();
            services.AddScoped<IBlackListedTokensServices, BlackListedTokensServices>();

            return services;
        }


    }
}
