using LMS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LMS.MVC.Services.Contracts.Services;
using LMS.MVC.Models.ViewModels.Home;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Category;
using Domain.Enums;
using LMS.MVC.Models.ViewModels.Course;

namespace LMS.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public HomeController(
            ILogger<HomeController> logger,
            ICourseService courseService,
            ICategoryService categoryService,
            IUserService userService)
        {
            _logger = logger;
            _courseService = courseService;
            _categoryService = categoryService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var coursesResponse = await _courseService.GetAllCoursesAsync();
            var categories = await _categoryService.GetAllCategories();
            var userCounts = await _userService.GetUserCounts();

            var courses = coursesResponse.Data ?? Enumerable.Empty<ReadCourseResult>();
            
            var viewModel = new HomeViewModel
            {
                TotalStudents = userCounts.StudentCount,
                TotalInstructors = userCounts.InstructorCount,
                TotalCourses = courses.Count(),
                Categories = categories.Take(6).Select(c => new LMS.BusinessLogic.DTOs.Category.ReadCategoryDTO 
                { 
                    Id = c.Id, 
                    Name = c.Name, 
                    CoursesCount = c.CoursesCount 
                }),
                PopularCourses = courses.OrderByDescending(c => c.EnrolledStudentsCount).Take(6).Select(c => new LMS.BusinessLogic.DTOs.Course.GetCourseDTO
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    Description = c.Description,
                    ThumbnailPath = c.ThumbnailPath,
                    IsFree = c.IsFree,
                    Price = c.Price,
                    AdminName = c.AdminName,
                    AverageRating = c.AverageRating,
                    EnrolledStudentsCount = c.EnrolledStudentsCount,
                    Language = c.Language
                })
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}