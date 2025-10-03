using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class TestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
