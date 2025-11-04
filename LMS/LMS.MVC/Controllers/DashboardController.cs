using LMS.MVC.Services.Contracts;
using LMS.MVC.Services.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfServices _services;
        public DashboardController(IUnitOfServices services)
        {
            _services = services;
        }
        private ICategoryService CategoryService => _services.CategoryService;
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                var users = await _services.UserService.GetAllUsers();
                return View("Admin/UserManagement", users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View("Admin/UserManagement", new List<object>());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CategoryManagement()
        {
            try
            {
                var categories = await CategoryService.GetAllCategories();
                return View("Admin/CategoryManagement", categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardController] Error in UserManagement: {ex.Message}");
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View("Admin/CategoryManagement", new List<object>());
            }
        }
    }
}