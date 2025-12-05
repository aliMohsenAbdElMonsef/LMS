using LMS.MVC.Models.ViewModels.Profile;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                
                var result = await _services.ProfileService.GetProfileAsync(userId);
                
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Profile not found.";
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
                    // Refresh authentication cookie with updated username
                    await RefreshAuthenticationCookie(userId, model.UserName);
                    
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

        // Helper method to refresh authentication cookie
        private async Task RefreshAuthenticationCookie(string userId, string newUserName)
        {
            try
            {
                // Get current claims
                var currentClaims = User.Claims.ToList();
                
                // Create new claims list with updated username
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, newUserName ?? string.Empty),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, 
                        currentClaims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty)
                };

                // Add all role claims
                var roleClaims = currentClaims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role);
                claims.AddRange(roleClaims);

                var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
                    claims, 
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    IsPersistent = User.Identity.AuthenticationType == Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    AllowRefresh = true
                };

                // Sign in again with updated claims
                await HttpContext.SignInAsync(
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    new System.Security.Claims.ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
            }
            catch (Exception)
            {
                // Ignore errors during cookie refresh
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
