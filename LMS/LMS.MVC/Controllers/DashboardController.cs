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
            var users = await _services.UserService.GetAllUsers();

            return View("Admin/UserManagement", users);
        }
    }
}
