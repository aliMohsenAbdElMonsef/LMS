// Controllers/HomeController.cs
using LMS.MVC.Models;
using LMS.MVC.Models.ViewModels;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
                var viewModel = new HomeViewModel
                {
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