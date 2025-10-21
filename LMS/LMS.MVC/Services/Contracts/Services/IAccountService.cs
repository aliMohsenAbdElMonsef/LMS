using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Response;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IAccountService
    {
        Task<ServiceResult> RegisterUserAsync(SignUpViewModel model);
        Task<ServiceResult> LoginUserAsync(LoginViewModel model);

        //Task<ServiceResult> LogoutUserAsync();
    }
}
