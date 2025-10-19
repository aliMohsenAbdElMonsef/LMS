using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Application.DTOs.User
{
    public class ReadUserDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public UserType ApplyAs { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}
