using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace LMS.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfServices _services;

        public AccountController(IUnitOfServices services)
        {
            _services = services;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            var model = new SignUpViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _services.AccountService.RegisterUserAsync(model);

            if (result.Success)
                return RedirectToAction("Login");

            if (result.Errors != null && result.Errors.Count > 0)
            {
                foreach (var errorMsg in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, errorMsg);
                }
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please check your data.");
            }


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {

            try
            {
                await _services.AccountService.LogoutUserAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Logout failed: {ex.Message}");
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _services.AccountService.LoginUserAsync(model);
            if (result.Success)
                return RedirectToAction("Index", "Home");
            if (result.Errors != null && result.Errors.Count > 0)
            {
                foreach (var errorMsg in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, errorMsg);
                }
            }
            else if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials.");
            }
            return View(model);
        }

        [HttpPost("ApproveUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var result = await _services.AccountService.ApproveUser(id);

            if (result.Success)
                return RedirectToAction("UserManagement", "Dashboard");

            TempData["Error"] = result.Message;
            return RedirectToAction("UserManagement", "Dashboard");
        }

        [HttpPost("DenyUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyUser(string id)
        {
            var result = await _services.AccountService.DenyUser(id);
            if (result.Success)
                return RedirectToAction("UserManagement", "Dashboard");

            TempData["Error"] = result.Message;
            return RedirectToAction("UserManagement", "Dashboard");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            try
            {
                var result = await _services.AccountService.ForgotPasswordAsync(email);
                
                if (result.Success)
                {
                    ViewBag.SuccessMessage = result.Message;
                    ModelState.Clear(); 
                    return View();
                }

                ModelState.AddModelError("", result.Message ?? "Error sending reset link.");
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid password reset token.";
                return RedirectToAction("Login");
            }
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _services.AccountService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            if (result.Success)
            {
                TempData["Success"] = "Password reset successfully. Please login.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.Message ?? "Error resetting password.");
            return View(model);
        }
    }
}