using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.DTOs.User
{
    public class CreateUserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType ApplyAs { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public IFormFile? UserImage { get; set; } 
    }
}
