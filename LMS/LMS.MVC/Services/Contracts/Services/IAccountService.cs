using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Response;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IAccountService
    {
        Task<RegisterUserResult> RegisterUserAsync(SignUpViewModel model);
        Task<LoginServiceResult> LoginUserAsync(LoginViewModel model);

        //Task<ServiceResult> LogoutUserAsync();
    }
}
