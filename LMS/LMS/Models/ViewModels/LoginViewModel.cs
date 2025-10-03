using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]

        [Required(ErrorMessage ="Email is Required")]
        [EmailAddress(ErrorMessage ="Invalid Email Format")]
        public required string Email { get; set; }

        [Required(ErrorMessage ="Password is Required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
