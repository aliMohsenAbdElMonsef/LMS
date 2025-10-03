using LMS.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure custom cookie authentication
            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth", options =>
                {
                    options.LoginPath = "/Account/Login";          // Redirect if not logged in
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect if forbidden
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);    // Cookie lifetime
                    options.SlidingExpiration = true;                  // Reset expiration on activity
                });

            // Add MVC controllers with views
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            // Add Authentication & Authorization middleware
            app.UseAuthentication(); // first
            app.UseAuthorization();  // then authorization

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
