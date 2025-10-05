using LMS.Data;
using LMS.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace LMS.Controllers
{
    public class LecturesController : Controller
    {
        public ApplicationDbContext _context;
        public LecturesController(ApplicationDbContext contect)
        {
            _context = contect;
        }
        public async Task<IActionResult> MyLecture()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("login", "Account");
            }

                var lectures = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .SelectMany(e => e.Course.Lectures)
                    .Include(i => i.Course)
                    .ThenInclude(c => c.CourseInstructors)
                    .ThenInclude(ci => ci.Instructor)
                    .OrderBy(e => e.LectureDate)
                    .ToListAsync();

                return View("MyLecture",lectures);

        }
    }
}
