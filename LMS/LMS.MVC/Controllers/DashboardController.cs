using Domain.Enums;
using LMS.MVC.Models.ViewModels.User;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LMS.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfServices _services;
        public DashboardController(IUnitOfServices services)
        {
            _services = services;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                Console.WriteLine("[DashboardController] UserManagement called - attempting to get users");
                var users = await _services.UserService.GetAllUsers();
                Console.WriteLine($"[DashboardController] Successfully retrieved {users?.Count() ?? 0} users");
                return View("Admin/UserManagement", users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DashboardController] Error in UserManagement: {ex.Message}");
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View("Admin/UserManagement", new List<object>());
            }
        }

        [Authorize]
        public IActionResult TestAuth()
        {
            return Json(new { 
                Message = "Authentication successful", 
                User = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }


    }
}
