using Application.DTOs.User;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.DTOs.Auth;
using LMS.BusinessLogic.DTOs.Responses;
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

        Task<LoginResponseDTO> LoginUser(LoginDTO dto);

        Task<BasicResponseDTO> LogoutUser(string token, string userId);


    }
}
