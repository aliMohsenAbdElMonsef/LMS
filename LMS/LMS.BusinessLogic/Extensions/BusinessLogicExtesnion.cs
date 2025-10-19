using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Extensions
{
    public static class BusinessLogicExtesnion
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfServices, UnitOfServices>();
            services.AddScoped<ITokenServices, TokenServices>();
            services.AddScoped<IUserServices, UserServices>();
            return services;
        }
    }
}
