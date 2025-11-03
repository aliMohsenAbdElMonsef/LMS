using Application.DTOs.User;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.BusinessLogic.Services
{
    internal class UserServices : IUserServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenServices _tokenServices;
        private readonly IBlackListedTokensServices _blackListedTokensService;
        public UserServices(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ITokenServices tokenServices, IBlackListedTokensServices blackListedTokensService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenServices = tokenServices;
            _blackListedTokensService = blackListedTokensService;
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
            user.UserImage = Path.Combine("uploads", "users", "photos", "profile-images", fileName);
        }

        private static ReadUserDTO MapToReadUserDTO(ApplicationUser user)
        {
            ReadUserDTO dto = new ReadUserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email??"",
                UserImage = user.UserImage,
                ApplyAs = user.ApplyAs,
                Status = user.Status,
                username = user.UserName??"",
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
            var user = new ApplicationUser {
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
            var existuser = await _userManager.FindByEmailAsync(dto.Email);
            if(existuser != null)
            {
                response.Success = false;
                response.Message = "User with this email already exist.";
                response.UserId = existuser.Id;
            }
            else
            {
                var user = MapToApplicationUser(dto);

                CreateFile(dto.UserImage, user);

                var result = await _userManager.CreateAsync(user, dto.Password);


                if (result.Succeeded)
                {
                    response.Success = true;
                    response.Message = "User created successfully. Pending approval from admin.";
                    response.UserId = user.Id;
                }
                else
                {
                    response.Success = false;
                    response.Message = "User creation failed.";
                    response.Errors = result.Errors.Select(e => e.Description).ToList();
                }
            }
                

            return response;
        }


        public async Task<IEnumerable<ReadUserDTO>> GetAllUsers()
        {
            return await _userManager.Users.Select(u => MapToReadUserDTO(u)).ToListAsync();
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
            var user =  await _userManager.FindByIdAsync(userId);
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

            return roleResult;
        }

        public async Task<IdentityResult> DenyUserAsync(string userId)
        {
            var user =  await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            user.Status = ApplicationStatus.Rejected;
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            return await _userManager.UpdateAsync(user);
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

            if(user == null)
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
    }
}