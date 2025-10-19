using LMS.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities.MainEntities;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult SignUp()
    {
        var model = new SignUpViewModel();
        return View(model);
    }

    [HttpPost]
    public IActionResult SignUp(SignUpViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Handle successful sign-up logic here
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }


}
