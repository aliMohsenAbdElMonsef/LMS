using DataAccess.Context;
using Domain.Entities.MainEntities;
using LMS.DataAcess.Contracts;
using LMS.DataAcess.Repositories;
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.DataAcess.Extensions
{
    public static class DataAcessExtention
    {
        public static IServiceCollection AddDataAcessServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            string connectionString = config.GetConnectionString("LMSDb");
            services.AddDbContext<LMSDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<LMSDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }
}