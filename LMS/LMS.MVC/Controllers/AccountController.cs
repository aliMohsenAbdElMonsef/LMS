using LMS.MVC.Models.ViewModels.Account;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Logout()
    {


        return RedirectToAction("Login", "Account");
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

    [HttpGet]
    public IActionResult TestCookies()
    {
        var cookies = Request.Cookies;
        var cookieInfo = new List<object>();
        
        foreach (var cookie in cookies)
        {
            cookieInfo.Add(new { Name = cookie.Key, Value = cookie.Value });
        }
        
        return Json(new { 
            Message = "Cookie test", 
            Cookies = cookieInfo,
            Count = cookies.Count 
        });
    }
}
