using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Response;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IAccountService
    {
        Task<RegisterUserResult> RegisterUserAsync(SignUpViewModel model);
        Task<LoginServiceResult> LoginUserAsync(LoginViewModel model);
        Task<ApproveSerivceResult> ApproveUser(string id);
        Task<DenySerivceResult> DenyUser(string id);
        Task<bool> LogoutUserAsync();
        Task<BasicServiceResult> ForgotPasswordAsync(string email);
        Task<BasicServiceResult> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
