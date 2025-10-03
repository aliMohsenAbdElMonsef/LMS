using LMS.Data;
using LMS.Models.DataModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CoursesController : Controller
    {
        ApplicationDbContext LMS = new ApplicationDbContext();

        public IActionResult createCourse()
        {
            var instructors = LMS.Users
                         .Include(u => u.UserRole)
                         .ThenInclude(ur => ur.Role)
                         .Where(u => u.UserRole.Role.RoleName == "Instructor")
                         .ToList();

            return View("createCourse", instructors);
        }
        public async Task<IActionResult> SavecreateCourse(course course, IFormFile photo)
        {
            if (course.Skills != null)
            {
                course.Skills = course.Skills
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                    .ToList();
            }

            // PROBLEM 2: Skills might not have CourseId set
            // Make sure each skill is linked to the course
            if (course.Skills != null)
            {
                foreach (var skill in course.Skills)
                {
                    skill.CourseId = course.Id; // This will be 0 for new course
                    skill.Course = course; // Set navigation property
                }
            }



            // Process PhotoFile
            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photo.CopyToAsync(memoryStream);
                    course.PhotoData = memoryStream.ToArray();
                    course.PhotoFileName = photo.FileName;
                    course.PhotoContentType = photo.ContentType;
                    // course.Skills.
                }
            }
            LMS.courses.Add(course);
            await LMS.SaveChangesAsync();
            return RedirectToAction("AllCourses");
        }
        public IActionResult AllCourses()
        {
            return View();
        }
        public IActionResult MyCourses()
        {
            return View();
        }


        public IActionResult CourseDetails(int id)
        {
            course course = LMS.courses
                               .Include(c => c.Instructor)
                              .Include(c => c.Skills)
                              .FirstOrDefault(c => c.Id == id);
            return View("CourseDetails", course);
        }

        public IActionResult EditCourse(int id)
        {
            var instructors = LMS.Users
                         .Include(u => u.UserRole)
                         .ThenInclude(ur => ur.Role)
                         .Where(u => u.UserRole.Role.RoleName == "Instructor")
                         .ToList();
            course course = LMS.courses
                     .Include(c => c.Skills)
                     .FirstOrDefault(c => c.Id == id);
            editCourseViewModel vm = new editCourseViewModel();
            vm.Id = course.Id;
            vm.Name = course.Name;
            vm.Description = course.Description;
            vm.Prerequisites = course.Prerequisites;
            vm.Price = course.Price;
            vm.LastUpdated = course.LastUpdated;
            vm.DurationHours = course.DurationHours;
            vm.InstructorId = course.InstructorId;
            vm.EnrolledCount = course.EnrolledCount;
            vm.AverageRating = course.AverageRating;
            vm.TotalReviews = course.TotalReviews;
            vm.Instructor = course.Instructor;
            vm.Instructors = instructors;
            vm.Skills = course.Skills;

            return View("EditCourse", vm);
        }
        public async Task<IActionResult> SaveEditCourse(course course, IFormFile photo)
        {
            course courseFromDb = LMS.courses.Find(course.Id);
            courseFromDb.Name = course.Name;
            courseFromDb.Description = course.Description;
            courseFromDb.Prerequisites = course.Prerequisites;
            courseFromDb.Price = course.Price;
            courseFromDb.DurationHours = course.DurationHours;
            courseFromDb.LastUpdated = DateTime.Now.ToString("MMMM yyyy");
            if (course.InstructorId.HasValue)
            {

                User user = LMS.Users.Find(course.InstructorId);
                courseFromDb.Instructor = user;
                courseFromDb.InstructorId = course.InstructorId;

            }
            else
            {
                courseFromDb.Instructor = null;
                courseFromDb.InstructorId = null;

            }

            if (course.Skills!=null)
            {
                courseFromDb.Skills = course.Skills;

            }
            else
            {

            }


            //skills

            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await photo.CopyToAsync(memoryStream);
                    courseFromDb.PhotoFileName = photo.FileName;
                    courseFromDb.PhotoContentType = photo.ContentType;
                    courseFromDb.PhotoData = memoryStream.ToArray();
                }
            }
            LMS.SaveChanges();
            return RedirectToAction("CourseDetails", new { course.Id });
        }
        public IActionResult GetCoursePhoto(int id)
        {
            var course = LMS.courses.Find(id);
            if (course?.PhotoData == null)
                return NotFound();

            return File(course.PhotoData, course.PhotoContentType);
        }


    }
}
