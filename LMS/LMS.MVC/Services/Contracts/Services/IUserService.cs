
using LMS.MVC.Models.ViewModels.User;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IUserService: IBaseMVCServices
    {
        Task<IEnumerable<UserViewModel>> GetAllUsers();
    }
}
