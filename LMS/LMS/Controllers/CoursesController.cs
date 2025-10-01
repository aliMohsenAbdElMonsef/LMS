using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult AllCourses()
        {
            return View();
        }
        public IActionResult MyCourses() { 
            return View();
        }
    }
}
