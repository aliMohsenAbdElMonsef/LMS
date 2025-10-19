using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Auth
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email or username is required")]
        [Display(Name = "Email or Username")]
        [StringLength(100, ErrorMessage = "Email or username cannot exceed 100 characters")]
        public string EmailOrUserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool rememberMe { get; set; } = false;
    }
}