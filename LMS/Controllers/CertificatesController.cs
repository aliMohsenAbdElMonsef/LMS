using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class CertificatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
