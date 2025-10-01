using LMS.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult ManageUsers()
        {
            var PendingUsers = _userManager.Users.Where(user => user.Status == "Pending").ToList();
            return View(PendingUsers);
        }
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null) return NotFound();
            user.Status = "Done";
            await _userManager.UpdateAsync(user);
            if(!string.IsNullOrEmpty(user.ApplyAs))
                await _userManager.AddToRoleAsync(user, user.ApplyAs);
            return RedirectToAction("ManageUsers");
        }
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.Status = "Rejected";
            await _userManager.UpdateAsync(user);
            return RedirectToAction("ManageUsers");
        }
    }
}
