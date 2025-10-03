
using LMS.Data;
using LMS.Models;
using LMS.Models.DataModels;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Student = "Student";
    public const string Instructor = "Instructor";
}

public class Program
{
    private static async Task SeedDataAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { RoleNames.Admin, RoleNames.Student, RoleNames.Instructor };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        var adminEmail = "3lmny@firm.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "3lmny",
                LastName = "Admin",
                EmailConfirmed = true,
                ApplyAs = "Admin",
                Status = "Done",

            };

            var result = await userManager.CreateAsync(adminUser, "3lmny135792468"); 
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }
        }
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // DB
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Identity
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
        })
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            googleOptions.CallbackPath = "/signin-google";
            googleOptions.SaveTokens = true;
        });
        builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddTransient<EmailService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession();
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Seed roles
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            string[] roleNames = { "Student", "Instructor", "Admin" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        await SeedDataAsync(app);

        app.Run();
    }
}

