using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class UpdateUserDTO
    {
        [Required]
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public UserType? ApplyAs { get; set; }
        public ApplicationStatus? Status { get; set; }
        public IFormFile? UserImage { get; set; }
    }
}
