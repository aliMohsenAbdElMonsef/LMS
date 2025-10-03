using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class DiscussionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
