using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    public class UnitOfServices : IUnitOfServices
    {
        public IAccountService AccountService { get; }
        public IUserService UserService { get; }

        public UnitOfServices(IAccountService accountService, IUserService userService)
        {
            AccountService = accountService;
            UserService = userService;
        }
    }
}
