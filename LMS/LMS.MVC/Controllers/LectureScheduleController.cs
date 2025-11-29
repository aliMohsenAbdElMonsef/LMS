using AutoMapper;
using LMS.BusinessLogic.DTOs.LectureSchedule;
using LMS.MVC.Models.ViewModels.LectureSchedule;
using LMS.MVC.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LectureScheduleController : Controller
    {
        private readonly IUnitOfServices _services;
        private readonly IMapper _mapper;

        public LectureScheduleController(IUnitOfServices services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string courseId)
        {
            var result = await _services.LectureScheduleService.GetCourseSchedulesAsync(courseId);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            ViewBag.CourseId = courseId;
            // Fetch course name for display
            var course = await _services.CourseService.GetCourseDetails(Guid.Parse(courseId));
            ViewBag.CourseName = course.Data?.Name ?? "Unknown Course";

            return View(result.Data);
        }

        public async Task<IActionResult> Create(string courseId)
        {
            var course = await _services.CourseService.GetCourseDetails(Guid.Parse(courseId));
            // Fetch instructors (Admins + Instructors)
            var instructors = await _services.UserService.GetInstructorsAsync(); // Assuming this method exists or similar
            ViewBag.Instructors = instructors.Data;

            var model = new CreateLectureScheduleViewModel
            {
                CourseId = courseId,
                CourseName = course.Data?.Name,
                Schedules = new List<DayScheduleViewModel> { new DayScheduleViewModel { DurationMinutes = 90 } }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLectureScheduleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new CreateLectureScheduleDTO
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Schedules = model.Schedules.Select(s => new LMS.BusinessLogic.DTOs.LectureSchedule.DayScheduleDTO
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    DurationMinutes = s.DurationMinutes,
                    InstructorId = s.InstructorId
                }).ToList()
            };

            var result = await _services.LectureScheduleService.CreateScheduleAsync(dto);
            if (result.Success)
            {
                TempData["Success"] = "Schedule created successfully";
                return RedirectToAction("Index", new { courseId = model.CourseId });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var result = await _services.LectureScheduleService.GetScheduleByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            var model = new EditLectureScheduleViewModel
            {
                Id = result.Data.Id,
                CourseId = result.Data.CourseId,
                CourseName = result.Data.CourseName,
                Title = result.Data.Title,
                Description = result.Data.Description,
                DayOfWeek = result.Data.DayOfWeek,
                StartTime = result.Data.StartTime,
                DurationMinutes = result.Data.DurationMinutes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditLectureScheduleViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new UpdateLectureScheduleDTO
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                DurationMinutes = model.DurationMinutes
            };

            var result = await _services.LectureScheduleService.UpdateScheduleAsync(dto);
            if (result.Success)
            {
                TempData["Success"] = "Schedule updated successfully";
                return RedirectToAction("Index", new { courseId = model.CourseId });
            }

            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id, string courseId)
        {
            var result = await _services.LectureScheduleService.DeleteScheduleAsync(id);
            if (result.Success)
            {
                TempData["Success"] = "Schedule deleted successfully";
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index", new { courseId });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateLectures(string id, string courseId)
        {
            var result = await _services.LectureScheduleService.GenerateLecturesFromScheduleAsync(id);
            if (result.Success)
            {
                TempData["Success"] = "Lectures generated successfully";
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index", new { courseId });
        }
    }
}
