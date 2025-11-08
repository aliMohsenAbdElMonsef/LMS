using AutoMapper;
using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace LMS.MVC.Services.Services
{
    public class UnitOfServices : IUnitOfServices
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly Lazy<IAccountService> _accountService;
        private readonly Lazy<IUserService> _userService;
        private readonly Lazy<ICourseService> _courseService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly Lazy<IEnrollmentService> _enrollmentService;

        public UnitOfServices(HttpClient client, IHttpContextAccessor httpContextAccessor, ITokenService tokenService,ILoggerFactory loggerFactory, IMapper mapper)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _loggerFactory = loggerFactory;
            _mapper = mapper;
            _tokenService = tokenService;
            _accountService = new Lazy<IAccountService>(() => new AccountServices(_client, _httpContextAccessor, _loggerFactory.CreateLogger<AccountServices>(),_tokenService));
            _userService = new Lazy<IUserService>(() => new UserServices(_client,_httpContextAccessor));
            _courseService = new Lazy<ICourseService>(() => new CourseService(_client, _httpContextAccessor));
            _categoryService = new Lazy<ICategoryService>(() => new CategoryService(_client,_httpContextAccessor,_mapper,_tokenService));
            _enrollmentService = new Lazy<IEnrollmentService>(()=> new EnrollmentService(_client,_httpContextAccessor,_tokenService));


        }

        public IAccountService AccountService => _accountService.Value;
        public IUserService UserService => _userService.Value;
        public ICourseService CourseService => _courseService.Value;
        public ICategoryService CategoryService => _categoryService.Value;
        public IEnrollmentService EnrollmentService => _enrollmentService.Value;
    }
}
