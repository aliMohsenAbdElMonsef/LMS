using System.Text;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Services.Handlers;
using LMS.MVC.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace LMS.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================================================================
            // MVC Core Setup
            // ================================================================
            builder.Services.AddControllersWithViews();

            // ================================================================
            // Session
            // ================================================================
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ================================================================
            // Access HttpContext (needed for cookies in TokenService)
            // ================================================================
            builder.Services.AddHttpContextAccessor();

            // ================================================================
            // Token & Auth Handler
            // ================================================================
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<AuthHeaderHandler>();

            // ================================================================
            // HttpClient Configurations
            // ================================================================
            // Base client (used by TokenService)
            builder.Services.AddHttpClient("LMS.API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7033/");
            });

            // Example clients with automatic token attachment
            builder.Services.AddHttpClient<IUserService, UserServices>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7033/");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IAccountService, AccountServices>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7033/");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            // ================================================================
            // Custom Service Container
            // ================================================================
            builder.Services.AddScoped<IUnitOfServices, UnitOfServices>();

            // ================================================================
            // Cookie Authentication
            // ================================================================
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            // ================================================================
            // Optional: JWT validation (for MVC API calls, not required unless you validate MVC tokens)
            // ================================================================
            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LMS.API",
                    ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LMS.MVC",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKey")),
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };

                // Read token from cookies if necessary
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["AccessToken"];
                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

            // ================================================================
            // Build App
            // ================================================================
            var app = builder.Build();

            // ================================================================
            // Middleware Pipeline
            // ================================================================
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

            app.Run();
        }
    }
}
