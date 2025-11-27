using Application.DTOs.User;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.DTOs.User;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IUserServices 
    {
        
        Task<CreateUserResponseDTO> CreateUserAsync(SignUpDTO dto);
        
        Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role);
        
        Task<bool> RoleExistsAsync(string role);
        
        Task<IdentityResult> CreateRoleAsync(string role);
        
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        
        Task<IdentityResult> ApproveUserAsync(string userId);

        Task<IdentityResult> DenyUserAsync(string userId);

        Task<IEnumerable<ReadUserDTO>> GetAllUsers();
        
        Task<IEnumerable<ReadUserDTO>> GetPendingUsers();
        
        Task<IEnumerable<ReadUserDTO>> GetCurrentUsers();

        Task<ServiceResponseDTO<ReadUserDTO?>> FindByIdAsync(string id);

        Task<string?> GetUserName(string id);

        Task<LoginResponseDTO> LoginUser(LoginDTO dto);

        Task<BasicResponseDTO> LogoutUser(string token, string userId);

        Task<BasicResponseDTO> ForgotPasswordAsync(ForgotPasswordDTO dto);

        Task<BasicResponseDTO> ResetPasswordAsync(ResetPasswordDTO dto);

        Task<ServiceResponseDTO<ReadUserDTO>> UpdateProfileAsync(string userId, UpdateUserDTO dto);

        Task<BasicResponseDTO> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task<ServiceResponseDTO<UserStatsDTO>> GetUserStatsAsync(string userId);

    }
}
