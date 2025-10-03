using LMS.Data;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.HelpingMethods;
using System.Security.Claims;

namespace LMS.Controllers
{
    public class AccountController : Controller
    {
        public readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        //public async Task<IActionResult> Login(LoginViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        //    if (user == null) 
        //    {
        //        ModelState.AddModelError("", "Invalid Email");
        //        return View(model);
        //    }
        //    string enteredPasswordHash = Helper.GetHash(model.Password);

        //    if ( user.PasswordHash != enteredPasswordHash)
        //    {
        //        ModelState.AddModelError("", "Incorrect Password");
        //        return View(model);
        //    }

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name,user.Email),
        //        new Claim (ClaimTypes.Role,user.Role),
        //        new Claim("FullName", $"{user.FirstName}+\" \"+{user.LastName}")
        //    };
        //    var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
        //    var principal = new ClaimsPrincipal(identity);
        //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        //       new AuthenticationProperties
        //       {
        //           IsPersistent = model.RememberMe,
        //           ExpiresUtc = DateTime.UtcNow.AddHours(1)
        //       });
        //    return RedirectToAction("Index", "Home");
        //}
        public IActionResult Signup()
        {
            return View();
        }
        public IActionResult Manage()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


    }
}
