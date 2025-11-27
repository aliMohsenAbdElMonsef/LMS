using Application.DTOs.User;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.DTOs.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.BusinessLogic.Services
{
    public class UserServices : IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenServices _tokenServices;
        private readonly IBlackListedTokensServices _blackListedTokensService;
        private readonly IEmailService _emailService;

        public UserServices(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ITokenServices tokenServices, IBlackListedTokensServices blackListedTokensService, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenServices = tokenServices;
            _blackListedTokensService = blackListedTokensService;
            _emailService = emailService;
        }

        private static void CreateFile(IFormFile? file, ApplicationUser user)
        {
            if (file == null || file.Length == 0)
            {
                user.UserImage = "uploads/users/photos/profile-images/default.jpg";
                return;
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users", "photos", "profile-images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            user.UserImage = $"/uploads/users/photos/profile-images/{fileName}";
        }

        private static ReadUserDTO MapToReadUserDTO(ApplicationUser user)
        {
            ReadUserDTO dto = new ReadUserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                UserImage = user.UserImage,
                ApplyAs = user.ApplyAs,
                Status = user.Status,
                username = user.UserName ?? "",
                Bio = user.Bio,
                Country = user.Country,
                City = user.City,
                PhoneNumber = user.PhoneNumber,
                IsDeleted = user.IsDeleted
            };
            if (user.IsDeleted)
            {
                dto.DeletedAt = user.DeletedAt;
            }
            return dto;
        }

        private ApplicationUser MapToApplicationUser(SignUpDTO dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                ApplyAs = (UserType)dto.ApplyAs,
                Status = ApplicationStatus.Pending,
                IsDeleted = false
            };
            return user;
        }

        public async Task<CreateUserResponseDTO> CreateUserAsync(SignUpDTO dto)
        {
            var response = new CreateUserResponseDTO();

            var existingUser = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            var existingUsername = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

            if (existingUser != null || existingUsername != null)
            {
                var user = existingUser ?? existingUsername;

                if (user.IsDeleted)
                {
                    user.IsDeleted = false;
                    user.Status = Domain.Enums.ApplicationStatus.Pending;

                    CreateFile(dto.UserImage, user);

                    await _userManager.UpdateAsync(user);

                    response.Success = true;
                    response.Message = "Account reactivated. Pending approval from admin.";
                    response.UserId = user.Id;
                }
                else
                {
                    response.Success = false;
                    response.Message = "A user with this email or username already exists.";
                }

                return response;
            }

            var newUser = MapToApplicationUser(dto);

            CreateFile(dto.UserImage, newUser);

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (result.Succeeded)
            {
                response.Success = true;
                response.Message = "User created successfully. Pending approval from admin.";
                response.UserId = newUser.Id;

                // Send Welcome Email
                try
                {
                    string subject = "Welcome to LMS - Registration Successful";
                    string body = $"<h3>Hello {newUser.FirstName},</h3><p>Your registration was successful. Your account is currently <b>Pending Approval</b> by an administrator.</p><p>You will receive another email once your account is approved.</p>";
                    await _emailService.SendEmailAsync(newUser.Email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send welcome email: {ex.Message}");
                }
            }
            else
            {
                response.Success = false;
                response.Message = "User creation failed.";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
            }

            return response;
        }

        public async Task<IEnumerable<ReadUserDTO>> GetAllUsers()
        {
            return await _userManager.Users
                .IgnoreQueryFilters()
                .Select(u => MapToReadUserDTO(u))
                .ToListAsync();
        }

        public async Task<LoginResponseDTO> LoginUser(LoginDTO dto)
        {
            var user = await _userManager.FindByNameAsync(dto.EmailOrUserName)
                     ?? await _userManager.FindByEmailAsync(dto.EmailOrUserName);

            if (user == null)
                return new LoginResponseDTO { Success = false, Message = "User not found" };

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                return new LoginResponseDTO { Success = false, Message = "Invalid password" };

            if (user.Status != ApplicationStatus.Approved)
                return new LoginResponseDTO { Success = false, Message = "User not approved by admin yet" };

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            var (accessToken, accessTokenExpiry) = await _tokenServices.GenerateAccessToken(user, roles);

            var (refreshToken, refreshTokenExpiry) = _tokenServices.GenerateRefreshToken();

            await _tokenServices.SaveRefreshTokenAsync(user, refreshToken, refreshTokenExpiry);

            var userDto = new ReadUserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserImage = user.UserImage,
                ApplyAs = user.ApplyAs,
                Status = user.Status,
                username = user.UserName ?? string.Empty,
                Bio = user.Bio,
                Country = user.Country,
                City = user.City,
                PhoneNumber = user.PhoneNumber,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt
            };

            return new LoginResponseDTO
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiry,
                Roles = roles,
                User = userDto
            };
        }

        public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<bool> RoleExistsAsync(string role)
        {
            return await _roleManager.RoleExistsAsync(role);
        }

        public async Task<IdentityResult> CreateRoleAsync(string role)
        {
            return await _roleManager.CreateAsync(new ApplicationRole(role));
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> ApproveUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }
            user.Status = ApplicationStatus.Approved;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return updateResult;
            }
            var roleName = Enum.GetName(typeof(UserType), user.ApplyAs);
            var roleResult = await AddUserToRoleAsync(user, roleName);

            // Send Approval Email
            try
            {
                string subject = "LMS Account Approved";
                string body = $"<h3>Congratulations {user.FirstName}!</h3><p>Your account has been <b>APPROVED</b>.</p><p>You can now log in to the system.</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send approval email: {ex.Message}");
            }

            return roleResult;
        }

        public async Task<IdentityResult> DenyUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            user.Status = ApplicationStatus.Rejected;
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            // Send Denial Email
            try
            {
                string subject = "LMS Account Application Update";
                string body = $"<h3>Hello {user.FirstName},</h3><p>We regret to inform you that your application for an LMS account has been <b>DENIED</b>.</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send denial email: {ex.Message}");
            }

            return result;
        }

        public async Task<IEnumerable<ReadUserDTO>> GetPendingUsers()
        {
            return await _userManager.Users
                .Where(u => u.Status == ApplicationStatus.Pending)
                .Select(u => MapToReadUserDTO(u))
                .ToListAsync();
        }

        public async Task<IEnumerable<ReadUserDTO>> GetCurrentUsers()
        {
            return await _userManager.Users
                .Where(u => u.Status == ApplicationStatus.Approved)
                .Select(u => MapToReadUserDTO(u))
                .ToListAsync();
        }

        public async Task<BasicResponseDTO> LogoutUser(string token, string userId)
        {
            var expiryDate = _tokenServices.GetExpiryFromToken(token);

            await _blackListedTokensService.AddTokenAsync(token, expiryDate, userId);

            return new BasicResponseDTO
            {
                Success = true,
                Message = "User logged out successfully"
            };
        }

        public async Task<ServiceResponseDTO<ReadUserDTO?>> FindByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ServiceResponseDTO<ReadUserDTO?> ErrorResponse = new ServiceResponseDTO<ReadUserDTO?>
                {
                    Data = null,
                    Success = false,
                    Message = "User not found."
                };
                return ErrorResponse;
            }
            var userDto = MapToReadUserDTO(user);
            ServiceResponseDTO<ReadUserDTO?> response = new ServiceResponseDTO<ReadUserDTO?>
            {
                Data = userDto,
                Success = true,
                Message = "User found successfully."
            };
            return response;
        }

        public async Task<string?> GetUserName(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user?.UserName;
        }

        public async Task<BasicResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new BasicResponseDTO { Success = true, Message = "If your email is registered, you will receive a password reset link." };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var encodedEmail = System.Web.HttpUtility.UrlEncode(dto.Email);
            var resetLink = $"{dto.ResetUrl}?token={encodedToken}&email={encodedEmail}";

            string subject = "Password Reset Request";
            string body = $"<h3>Hello {user.FirstName},</h3><p>Click the link below to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email: {ex.Message}");
                return new BasicResponseDTO { Success = false, Message = "Failed to send email." };
            }

            return new BasicResponseDTO { Success = true, Message = "If your email is registered, you will receive a password reset link." };
        }

        public async Task<BasicResponseDTO> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new BasicResponseDTO { Success = false, Message = "Invalid request." };
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (result.Succeeded)
            {
                return new BasicResponseDTO { Success = true, Message = "Password has been reset successfully." };
            }

            return new BasicResponseDTO
            {
                Success = false,
                Message = "Failed to reset password: " + string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<ServiceResponseDTO<ReadUserDTO>> UpdateProfileAsync(string userId, UpdateUserDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ServiceResponseDTO<ReadUserDTO> { Success = false, Message = "User not found." };
            }

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.UserName = dto.UserName ?? user.UserName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Bio = dto.Bio ?? user.Bio;
            user.Country = dto.Country ?? user.Country;
            user.City = dto.City ?? user.City;

            if (dto.UserImage != null)
            {
                CreateFile(dto.UserImage, user);
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ServiceResponseDTO<ReadUserDTO>
                {
                    Success = false,
                    Message = "Failed to update profile: " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new ServiceResponseDTO<ReadUserDTO>
            {
                Success = true,
                Data = MapToReadUserDTO(user),
                Message = "Profile updated successfully."
            };
        }

        public async Task<BasicResponseDTO> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new BasicResponseDTO { Success = false, Message = "User not found." };
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                return new BasicResponseDTO
                {
                    Success = false,
                    Message = "Failed to change password: " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new BasicResponseDTO { Success = true, Message = "Password changed successfully." };
        }

        public async Task<ServiceResponseDTO<UserStatsDTO>> GetUserStatsAsync(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Enrollment)
                .Include(u => u.EarnedCertificates)
                .Include(u => u.UploadedAssignemnts)
                .Include(u => u.QuizzesAttended)
                .Include(u => u.CreatedCourses)
                .Include(u => u.CreatedAssignments)
                .Include(u => u.CreatedQuizzes)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new ServiceResponseDTO<UserStatsDTO> { Success = false, Message = "User not found." };
            }

            var stats = new UserStatsDTO
            {
                EnrolledCoursesCount = user.Enrollment.Count,
                CompletedCoursesCount = user.Enrollment.Count(e => e.progress >= 100),
                CertificatesCount = user.EarnedCertificates.Count,
                AssignmentsSubmitted = user.UploadedAssignemnts.Count,
                QuizzesTaken = user.QuizzesAttended.Count,
                AverageScore = 0,
                CreatedCoursesCount = user.CreatedCourses.Count,
                TotalStudents = 0,
                AverageRating = 0
            };

            return new ServiceResponseDTO<UserStatsDTO>
            {
                Success = true,
                Data = stats,
                Message = "Stats retrieved successfully."
            };
        }
    }
}
