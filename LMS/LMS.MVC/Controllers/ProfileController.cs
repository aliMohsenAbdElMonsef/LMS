using LMS.MVC.Models.ViewModels.Profile;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUnitOfServices _services;

        public ProfileController(IUnitOfServices services)
        {
            _services = services;
        }

        // GET: Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User ID not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }
                
                Console.WriteLine($"🔍 ProfileController.Index - UserId: {userId}");
                
                var result = await _services.ProfileService.GetProfileAsync(userId);
                
                Console.WriteLine($"🔍 ProfileController.Index - Result Success: {result.Success}, Data: {result.Data != null}");
                
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Profile not found.";
                    Console.WriteLine($"❌ ProfileController.Index - Error: {result.Message}");
                    return RedirectToAction("Index", "Home");
                }
                
                // Fetch stats
                var statsResult = await _services.ProfileService.GetUserStatsAsync(userId);
                
                var viewModel = new ProfileIndexViewModel
                {
                    Profile = result.Data,
                    Stats = statsResult.Success && statsResult.Data != null ? statsResult.Data : new UserStatsViewModel()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ProfileController.Index - Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error loading profile: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var result = await _services.ProfileService.GetProfileAsync(userId);
                
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Profile not found.";
                    return RedirectToAction("Index");
                }
                
                // Map ProfileViewModel to UpdateProfileViewModel
                var updateModel = new UpdateProfileViewModel
                {
                    Id = result.Data.Id,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    Email = result.Data.Email,
                    UserName = result.Data.UserName,
                    PhoneNumber = result.Data.PhoneNumber,
                    Bio = result.Data.Bio,
                    Country = result.Data.Country,
                    City = result.Data.City,
                    ExistingUserImage = result.Data.UserImage
                };
                
                return View(updateModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading profile: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var result = await _services.ProfileService.UpdateProfileAsync(userId, model);
                
                if (result.Success)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Index");
                }
                
                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating profile: {ex.Message}";
                return View(model);
            }
        }

        // GET: Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var result = await _services.ProfileService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
                
                if (result.Success)
                {
                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction("Index");
                }
                
                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error changing password: {ex.Message}";
                return View(model);
            }
        }
    }
}
