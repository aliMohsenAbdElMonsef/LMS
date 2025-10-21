using LMS.BusinessLogic.Contracts.Services;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    public class UnitOfServices : IUnitOfServices
    {
        private readonly Lazy<IAccountService> _accountService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnitOfServices(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            _accountService = new Lazy<IAccountService>(() =>
                new AccountServices(_httpClientFactory, _httpContextAccessor));
        }

        public IAccountService AccountService => _accountService.Value;
    }

}
