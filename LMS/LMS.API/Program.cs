using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Seedings;
using LMS.BusinessLogic.Extensions;
using LMS.DataAccess.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.FileProviders;
using LMS.API.Services;
using Microsoft.OpenApi.Models;

namespace LMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---------------------- Services ----------------------
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDataAcessServices(builder.Configuration).AddBusinessLogicServices(builder.Configuration);
            builder.Services.AddHostedService<LMS.API.BackgroundServices.LectureReminderBackgroundService>();
            builder.Services.AddScoped<IFileUploadService, Services.FileUploadService>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMvc",
                    builder => builder.WithOrigins(
                        "https://localhost:5120",
                        "http://localhost:5119",
                        "https://localhost:7001",
                        "http://localhost:5000"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("*"));
            });

            // ---------------------- Swagger ----------------------
            builder.Services.AddSwaggerGen(c =>
            {
                c.UseInlineDefinitionsForEnums();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {token}'"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // ---------------------- JWT Authentication ----------------------
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; 

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[API JWT] ❌ Authentication failed: {context.Exception.Message}");
                        Console.WriteLine($"[API JWT] Exception: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"[API JWT] ✅ Token validated for user: {context.Principal.Identity?.Name}");
                        var roles = context.Principal.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();
                        Console.WriteLine($"[API JWT] Roles found: {string.Join(", ", roles)}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        // Check both header and query string for token
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                        Console.WriteLine($"[API JWT] 📨 Token from header: {!string.IsNullOrEmpty(token)}");

                        if (string.IsNullOrEmpty(token))
                        {
                            token = context.Request.Query["access_token"];
                            Console.WriteLine($"[API JWT] 📨 Token from query: {!string.IsNullOrEmpty(token)}");
                        }

                        context.Token = token;
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        Console.WriteLine($"[API JWT] 🚫 Access forbidden for: {context.HttpContext.User.Identity?.Name}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"[API JWT] 🚨 Challenge issued: {context.Error} - {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ---------------------- Build App ----------------------
            var app = builder.Build();

            // ---------------------- Seed Admin ----------------------
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                await IdentitySeeding.SeedAdminAsync(userManager, roleManager);
                
                // [ADDED] Create uploads directory
                var webHostEnvironment = services.GetRequiredService<IWebHostEnvironment>();
                var uploadsPath = Path.Combine(webHostEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    Directory.CreateDirectory(Path.Combine(uploadsPath, "assignments"));
                    Directory.CreateDirectory(Path.Combine(uploadsPath, "submissions"));
                    Console.WriteLine("✅ API Uploads directories created successfully");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowMvc");
            
            // [ADDED] Serve static uploads directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "uploads")),
                RequestPath = "/uploads",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600");
                }
            });
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}