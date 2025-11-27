namespace LMS.BusinessLogic.DTOs.Auth
{
    public class ForgotPasswordDTO
    {
        public string Email { get; set; }
        public string ResetUrl { get; set; }
    }
}
