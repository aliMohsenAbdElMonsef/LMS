using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Contracts
{
    public interface IUnitOfServices
    {
        IAccountService AccountService { get; }
    }
}
