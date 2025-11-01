using AutoMapper;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;

namespace LMS.MVC.Services.Services
{
    public class UnitOfServices : IUnitOfServices
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMapper _mapper;
        private readonly Lazy<IAccountService> _AccountService;
        private readonly Lazy<IUserService> _UserService;
        private readonly Lazy<ICourseService> _CourseService;
        private readonly Lazy<ICategoryService> _CategoryService;

        public UnitOfServices(HttpClient client, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, ILoggerFactory loggerFactory, IMapper mapper)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
            _loggerFactory = loggerFactory;
            _mapper = mapper;
            _AccountService = new Lazy<IAccountService>(() => new AccountServices(_client, _httpContextAccessor, _loggerFactory.CreateLogger<AccountServices>()));
            _CategoryService = new Lazy<ICategoryService>(() => new CategoryService(_client, _tokenService, _mapper));
            _UserService = new Lazy<IUserService>(() => new UserServices(_client));
            _CourseService = new Lazy<ICourseService>(() => new CourseService(_client, _tokenService));
        }

        public IAccountService AccountService => _AccountService.Value;
        public IUserService UserService => _UserService.Value;
        public ICourseService CourseService => _CourseService.Value;
        public ICategoryService CategoryService => _CategoryService.Value;
    }
}
