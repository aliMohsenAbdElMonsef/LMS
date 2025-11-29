using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    public class LectureReminderService : ILectureReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public LectureReminderService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task SendLectureRemindersAsync()
        {
            // Find lectures starting in 10-15 minutes
            var now = DateTime.UtcNow;
            var reminderWindowStart = now.AddMinutes(10);
            var reminderWindowEnd = now.AddMinutes(15);

            // Note: This query might need adjustment based on how LectureDate and StartTime are stored
            // Assuming LectureDate stores the date and StartTime stores the time of day
            
            var lectures = await _unitOfWork.Lectures.GetQueryable()
                .Include(l => l.Course)
                .Include(l => l.Course.Students).ThenInclude(se => se.Student)
                .Include(l => l.Instructor)
                .Where(l => !l.IsDeleted)
                .ToListAsync();

            var upcomingLectures = lectures.Where(l => 
            {
                var lectureStartDateTime = l.LectureDate.Date + l.StartTime;
                return lectureStartDateTime >= reminderWindowStart && lectureStartDateTime <= reminderWindowEnd;
            }).ToList();

            foreach (var lecture in upcomingLectures)
            {
                // Send email to instructor
                if (lecture.Instructor != null)
                {
                    var subject = $"Reminder: Lecture for {lecture.Course.Name} starts in 10 minutes";
                    var body = $@"
                        <h2>Lecture Reminder</h2>
                        <p>Hello {lecture.Instructor.UserName},</p>
                        <p>Your lecture for <strong>{lecture.Course.Name}</strong> is starting in 10 minutes.</p>
                        <p><strong>Time:</strong> {lecture.StartTime}</p>
                        <p>Please be ready to launch the lecture.</p>";

                    await _emailService.SendEmailAsync(lecture.Instructor.Email, subject, body);
                }

                // Send email to enrolled students
                foreach (var enrollment in lecture.Course.Students)
                {
                    if (enrollment.Student != null)
                    {
                        var subject = $"Reminder: Lecture for {lecture.Course.Name} starts in 10 minutes";
                        var body = $@"
                            <h2>Lecture Reminder</h2>
                            <p>Hello {enrollment.Student.UserName},</p>
                            <p>The lecture for <strong>{lecture.Course.Name}</strong> is starting in 10 minutes.</p>
                            <p><strong>Instructor:</strong> {lecture.Instructor?.UserName ?? "Unknown"}</p>
                            <p><strong>Time:</strong> {lecture.StartTime}</p>
                            <p>Please log in to the LMS to join.</p>";

                        await _emailService.SendEmailAsync(enrollment.Student.Email, subject, body);
                    }
                }
            }
        }
    }
}
