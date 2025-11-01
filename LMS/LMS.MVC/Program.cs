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

            // ---------------- MVC & Session ----------------
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();

            // ---------------- CORS ----------------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMvc", policy =>
                    policy.WithOrigins(
                        "https://localhost:5120",
                        "http://localhost:5119",
                        "https://localhost:7001",
                        "http://localhost:5000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7033/";

            // ---------------- Token & Auth ----------------
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<AuthHeaderHandler>();

            // ---------------- HttpClients ----------------
            builder.Services.AddHttpClient("LMS.API", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });

            builder.Services.AddHttpClient<IAccountService, AccountServices>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IUserService, UserServices>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<ICourseService, CourseService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(300);
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient<IUnitOfServices, UnitOfServices>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            // ---------------- JWT Authentication ----------------
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                    };

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

            // ---------------- AutoMapper ----------------
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ---------------- Build App ----------------
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowMvc");
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
