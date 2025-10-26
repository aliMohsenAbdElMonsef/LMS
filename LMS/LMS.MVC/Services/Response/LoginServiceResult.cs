using Application.DTOs.User;

namespace LMS.MVC.Services.Response
{
    public class LoginServiceResult : BasicServiceResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }

        public ReadUserDTO User { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}