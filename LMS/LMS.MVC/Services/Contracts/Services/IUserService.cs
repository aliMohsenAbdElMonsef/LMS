
using LMS.MVC.Models.ViewModels.User;
using LMS.BusinessLogic.DTOs.Responses;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface IUserService: IBaseMVCServices
    {
        Task<IEnumerable<UserViewModel>> GetAllUsers();
        Task<ServiceResponseDTO<UserViewModel>> GetUserById(string id);
    }
}
