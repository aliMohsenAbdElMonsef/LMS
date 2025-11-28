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
        private readonly Lazy<IAssignmentService> _assignmentService;
        private readonly Lazy<ILectureService> _lectureService;
        private readonly Lazy<IProfileService> _profileService;
        private readonly Lazy<IQuizService> _quizService;

        private readonly IConfiguration _configuration;

        public UnitOfServices(HttpClient client, IHttpContextAccessor httpContextAccessor, ITokenService tokenService,ILoggerFactory loggerFactory, IMapper mapper, IConfiguration configuration)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _loggerFactory = loggerFactory;
            _mapper = mapper;
            _tokenService = tokenService;
            _configuration = configuration;
            _accountService = new Lazy<IAccountService>(() => new AccountServices(_client, _httpContextAccessor, _loggerFactory.CreateLogger<AccountServices>(),_tokenService));
            _userService = new Lazy<IUserService>(() => new UserServices(_client,_httpContextAccessor, _configuration));
            _courseService = new Lazy<ICourseService>(() => new CourseService(_client, _httpContextAccessor));
            _categoryService = new Lazy<ICategoryService>(() => new CategoryService(_client,_httpContextAccessor,_mapper,_tokenService));
            
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7033/";
            _enrollmentService = new Lazy<IEnrollmentService>(()=> new EnrollmentService(_client,_httpContextAccessor,_tokenService, baseUrl));
            
            _assignmentService = new Lazy<IAssignmentService>(()=> new AssignmentService(_client,_httpContextAccessor,_tokenService, _mapper));
            _lectureService = new Lazy<ILectureService>(() => new LectureService(_client, _httpContextAccessor));
            _profileService = new Lazy<IProfileService>(() => new ProfileService(_client, _httpContextAccessor));
            _quizService = new Lazy<IQuizService>(() => new QuizService(_client, _httpContextAccessor));
        }

        public IAccountService AccountService => _accountService.Value;
        public IAssignmentService AssignmentService => _assignmentService.Value;
        public IUserService UserService => _userService.Value;
        public ICourseService CourseService => _courseService.Value;
        public ICategoryService CategoryService => _categoryService.Value;
        public IEnrollmentService EnrollmentService => _enrollmentService.Value;
        public ILectureService LectureService => _lectureService.Value;
        public IProfileService ProfileService => _profileService.Value;
        public IQuizService QuizService => _quizService.Value;
    }
}
