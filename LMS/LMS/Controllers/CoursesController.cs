using LMS.Data;
using LMS.Models.DataModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> AllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseInstructors)
                .ThenInclude(ci => ci.Instructor)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseInstructors)
                .ThenInclude(ci => ci.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            return View(course);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new CreateCourseViewModel
            {
                AllSkills = _context.Skills?.ToList() ?? new List<Skill>(),
                AllCategories = _context.Categories?.ToList() ?? new List<Category>(),
                AllCourses = _context.Courses?.ToList() ?? new List<Course>()
            };
            return View("CreateCourse",viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCourseViewModel courseVM)
        {
            if (!ModelState.IsValid)
            {
                courseVM.AllSkills = _context.Skills?.ToList() ?? new List<Skill>();
                return View("CreateCourse", courseVM);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                var course = new Course
                {
                    Name = courseVM.Name,
                    CourseCode = courseVM.CourseCode,
                    Description = courseVM.Description,
                    Credits = courseVM.Credits,
                    Level = courseVM.Level,
                    Language = courseVM.Language,
                    StartDate = courseVM.StartDate,
                    EndDate = courseVM.EndDate,
                    DurationWeeks = courseVM.DurationWeeks,
                    Price = courseVM.Price,
                    IsFree = courseVM.IsFree,
                    DeliveryMode = courseVM.DeliveryMode
                };

                if (courseVM.Thumbnail != null && courseVM.Thumbnail.Length > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "thumbnails");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(courseVM.Thumbnail.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await courseVM.Thumbnail.CopyToAsync(stream);
                    }

                    course.ThumbnailPath = $"/images/thumbnails/{fileName}";

                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                if (courseVM.SelectedSkillIds != null && courseVM.SelectedSkillIds.Any())
                {
                    var courseSkills = courseVM.SelectedSkillIds.Select(skillId => new CourseSkill
                    {
                        CourseId = course.Id,
                        SkillId = skillId
                    }).ToList();

                    _context.CourseSkills.AddRange(courseSkills);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", "An error occurred while creating the course.");
                courseVM.AllSkills = _context.Skills.ToList();
                return View("CreateCourse", courseVM);
            }
        }
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Edit(int id, Course updatedCourse)
        {
            if (id != updatedCourse.Id) return BadRequest();

            if (!ModelState.IsValid) return View(updatedCourse);

            _context.Courses.Update(updatedCourse);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignInstructor(int courseId, string instructorId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course not found");

            var instructor = await _userManager.FindByIdAsync(instructorId);
            if (instructor == null) return NotFound("Instructor not found");

            if (!await _userManager.IsInRoleAsync(instructor, "Instructor"))
                return BadRequest("User is not an Instructor");

            bool alreadyAssigned = await _context.CourseInstructors
                .AnyAsync(ci => ci.CourseId == courseId && ci.InstructorId == instructorId);
            if (alreadyAssigned)
                return BadRequest("Instructor already assigned to this course");

            _context.CourseInstructors.Add(new CourseInstructor
            {
                CourseId = courseId,
                InstructorId = instructorId
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = courseId });
        }
    }
}
