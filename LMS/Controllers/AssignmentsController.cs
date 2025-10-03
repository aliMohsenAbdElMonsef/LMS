using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class AssignmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
