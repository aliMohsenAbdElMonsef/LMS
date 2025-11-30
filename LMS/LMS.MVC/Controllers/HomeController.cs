using LMS.MVC.Models;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IUnitOfServices _services;

        public HomeController(ILogger<HomeController> logger, IUnitOfServices services)
        {
            _logger = logger;
            _services = services;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch stats (Local logic)
                var userCounts = await _services.UserService.GetUserCounts();
                var coursesResponse = await _services.CourseService.GetAllCoursesAsync();
                var totalCourses = coursesResponse.Data?.Count() ?? 0;

                var viewModel = new HomeViewModel
                {
                    TotalStudents = userCounts.StudentCount,
                    TotalInstructors = userCounts.InstructorCount,
                    TotalCourses = totalCourses,
                    
                    // Fetch lists using HomeService (Remote logic)
                    Categories = await _services.HomeService.GetTopCategoriesAsync(6),
                    PopularCourses = await _services.HomeService.GetPopularCoursesAsync(6)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");
                return View(new HomeViewModel());
            }
        }

        [AllowAnonymous]
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