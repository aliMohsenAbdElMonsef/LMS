using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

internal class LectureService : BaseServices<Lecture, GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>, ILectureServices
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public LectureService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService) : base(unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    protected override Lecture MapToEntity(CreateLectureDTO dto)
    {
        return _mapper.Map<Lecture>(dto);
    }

    protected override Lecture UpdateToEntity(UpdateLectureDTO dto, Lecture existingEntity)
    {
        _mapper.Map(dto, existingEntity);
        return existingEntity;
    }

    protected override GetLectureDTO MapToReadDTO(Lecture entity)
    {
        return _mapper.Map<GetLectureDTO>(entity);
    }

    protected override IBaseRepository<Lecture, string> GetRepo()
    {
        return _unitOfWork.Lectures;
    }

    protected override string GetIdFromUpdateDTO(UpdateLectureDTO dto)
    {
        return dto.Id;
    }

    private async Task<bool> CanAccessLecture(string courseid,string lectureId, string userId, string userRole)
    {
        if (string.IsNullOrEmpty(courseid))
        {
            var lectureTemp = await _unitOfWork.Lectures.FindByIdAsync(lectureId);
            if (lectureTemp == null) return false;
            courseid = lectureTemp.CourseId;
        }
        Course c = await _unitOfWork.Courses.FindByIdAsync(courseid);
        if (c == null) {
            return false;
        }
        else
        {
            var Lectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseid);
            bool ck = false;
            foreach (var Lecture in Lectures)
            {
                if(Lecture.Id == lectureId)
                {
                    ck = true;
                    break;
                }
            }
            if (!ck)
            {
                return false;
            }
        }
        if (userRole == "Admin") return true;

        var lecture = await _unitOfWork.Lectures.FindByIdAsync(lectureId);
        if (lecture == null) return false; 

        if (userRole == "Instructor" && lecture.InstructorId == userId) return true;
        if (userRole == "Student")
        {
            var enrollment = await _unitOfWork.StudentEnrollments.GetFirstOrDefaultAsync(userId, lecture.CourseId);
            return enrollment != null;
        }


        return false;
    }

    private async Task<bool> CanAccessCourseLectures(string courseId, string userId, string userRole)
    {
        if (userRole == "Admin") 
            return true;

        var course = await _unitOfWork.Courses.FindByIdAsync(courseId);


        if (course == null) 
            return false;


        if (userRole == "Instructor")
        {
            var enrollment = await _unitOfWork.InstructorEnrollments.GetByInstructorAndCourseAsync(userId, courseId);
            return enrollment != null && enrollment.Status == Domain.Enums.ApplicationStatus.Approved;
        }

        if (userRole == "Student")
        {
            var enrollment = await _unitOfWork.StudentEnrollments.GetFirstOrDefaultAsync(userId, courseId);
            return enrollment != null;
        }

        return false;
    }

    private async Task<bool> CanManageLecture(string lectureId, string userId, string userRole)
    {
        if (userRole == "Admin") return true;

        var lecture = await _unitOfWork.Lectures.FindByIdAsync(lectureId);
        if (lecture == null) return false;

        if (userRole == "Instructor" && lecture.InstructorId == userId) return true;

        return false;
    }

    private async Task<bool> CanManageCourse(string courseId, string userId, string userRole)
    {
        if (userRole == "Admin") return true;


        return false;
    }

    private ServiceResponseDTO<T> ErrorResponse<T>(string message, List<string> errors = null)
    {
        return new ServiceResponseDTO<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    private ServiceResponseDTO<T> SuccessResponse<T>(T data, string message = "")
    {
        return new ServiceResponseDTO<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public async Task<ServiceResponseDTO<GetLectureDTO>> CreateAsync(CreateLectureDTO dto, string userId, string userRole)
    {
        try
        {
            if (!await CanManageCourse(dto.CourseId, userId, userRole))
            {
                return ErrorResponse<GetLectureDTO>("You are not authorized to create lectures for this course");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                return ErrorResponse<GetLectureDTO>("Start time must be before end time");
            }

            TimeSpan endTimeToCheck = dto.EndTime ?? (dto.DurationMinutes.HasValue 
                ? dto.StartTime.Add(TimeSpan.FromMinutes(dto.DurationMinutes.Value)) 
                : dto.StartTime.Add(TimeSpan.FromHours(1)));

            var conflictCheck = await CheckLectureConflictAsync(dto.CourseId, dto.LectureDate, dto.StartTime, endTimeToCheck);
            if (conflictCheck.Data)
            {
                return ErrorResponse<GetLectureDTO>(conflictCheck.Message);
            }

            var entity = _mapper.Map<Lecture>(dto);

            // Calculate Lecture Number
            var existingLectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(dto.CourseId);
            entity.LectureNumber = existingLectures.Count() + 1;

            await _unitOfWork.Lectures.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var readDto = _mapper.Map<GetLectureDTO>(entity);
            return SuccessResponse(readDto, "Lecture created successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<GetLectureDTO>($"Error creating lecture: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<GetLectureDTO>> UpdateAsync(UpdateLectureDTO dto, string userId, string userRole)
    {
        try
        {
            if (!await CanManageLecture(dto.Id, userId, userRole))
            {
                return ErrorResponse<GetLectureDTO>("You are not authorized to update this lecture");
            }

            if (dto.StartTime.HasValue && dto.EndTime.HasValue && dto.StartTime >= dto.EndTime)
            {
                return ErrorResponse<GetLectureDTO>("Start time must be before end time");
            }

            var existingLecture = await GetRepo().FindByIdAsync(dto.Id);
            if (existingLecture == null)
            {
                return ErrorResponse<GetLectureDTO>("Lecture not found");
            }

            DateTime dateToCheck = dto.LectureDate ?? existingLecture.LectureDate;
            TimeSpan startTimeToCheck = dto.StartTime ?? existingLecture.StartTime;
            TimeSpan endTimeToCheck = dto.EndTime ?? existingLecture.EndTime;

            var conflictCheck = await CheckLectureConflictAsync(
                existingLecture.CourseId, dateToCheck, startTimeToCheck, endTimeToCheck, dto.Id);

            if (conflictCheck.Data)
            {
                return ErrorResponse<GetLectureDTO>(conflictCheck.Message);
            }

            return await base.UpdateAsync(dto);
        }
        catch (Exception ex)
        {
            return ErrorResponse<GetLectureDTO>($"Error updating lecture: {ex.Message}");
        }
    }

    public  async Task<ServiceResponseDTO<GetLectureDTO>> GetByIdAsync(string courseid,string lectureid, string userId, string userRole)
    {
        try
        {
            if (!await CanAccessLecture(courseid,lectureid, userId, userRole))
            {
                return ErrorResponse<GetLectureDTO>("You are not authorized to access this lecture");
            }

            return await base.GetByIdAsync(lectureid);
        }
        catch (Exception ex)
        {
            return ErrorResponse<GetLectureDTO>($"Error retrieving lecture: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string id, string userId, string userRole)
    {
        if (!await CanManageLecture(id, userId, userRole))
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this lecture");
        }

        await base.DeleteAsync(id);
    }

    public async Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetCourseLecturesAsync(string courseId, string userId, string userRole)
    {
        try
        {
            if (!await CanAccessCourseLectures(courseId, userId, userRole))
            {
                return ErrorResponse<IEnumerable<GetLectureDTO>>("Access denied. You need an approved enrollment to view this course's lecture schedule.");
            }

            var lectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseId);
            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);

            return SuccessResponse<IEnumerable<GetLectureDTO>>(lectureDTOs, "Course lectures retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<IEnumerable<GetLectureDTO>>($"Error retrieving course lectures: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId, string userId, string userRole)
    {
        try
        {
            if (userRole != "Admin" && userId != instructorId)
            {
                return ErrorResponse<IEnumerable<GetLectureDTO>>("You can only view your own lectures");
            }

            var lectures = await _unitOfWork.Lectures.GetAllAsync();
            var filteredLectures = lectures.Where(l => l.InstructorId == instructorId);
            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(filteredLectures);

            return SuccessResponse<IEnumerable<GetLectureDTO>>(lectureDTOs, "Instructor lectures retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<IEnumerable<GetLectureDTO>>($"Error retrieving instructor lectures: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetUpcomingLecturesAsync(string courseId, string userId, string userRole)
    {
        try
        {
            if (!await CanAccessCourseLectures(courseId, userId, userRole))
            {
                return ErrorResponse<IEnumerable<GetLectureDTO>>("Access denied. You need an approved enrollment to view this course's lecture schedule.");
            }

            DateTime fromDate = DateTime.UtcNow.Date;
            var lectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseId);
            var upcomingLectures = lectures.Where(l => l.LectureDate >= fromDate).OrderBy(l => l.LectureDate);

            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(upcomingLectures);

            return SuccessResponse<IEnumerable<GetLectureDTO>>(lectureDTOs, "Upcoming lectures retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<IEnumerable<GetLectureDTO>>($"Error retrieving upcoming lectures: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetTodayLecturesAsync(string courseId, string userId, string userRole)
    {
        try
        {
            if (!await CanAccessCourseLectures(courseId, userId, userRole))
            {
                return ErrorResponse<IEnumerable<GetLectureDTO>>("Access denied. You need an approved enrollment to view this course's lecture schedule.");
            }

            var allLectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseId);
            var todayLectures = allLectures.Where(l => l.LectureDate.Date == DateTime.Today.Date);

            return SuccessResponse<IEnumerable<GetLectureDTO>>(
                _mapper.Map<IEnumerable<GetLectureDTO>>(todayLectures),
                "Today's lectures retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            return ErrorResponse<IEnumerable<GetLectureDTO>>($"Error retrieving today's lectures: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<bool>> CheckLectureConflictAsync(string courseId, DateTime date, TimeSpan startTime, TimeSpan endTime, string excludeLectureId = null)
    {
        try
        {
            if (startTime >= endTime)
            {
                return ErrorResponse<bool>("Start time must be before end time");
            }

            var lectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(courseId);
            
            // Check if there is any other lecture on the same day
            var conflictingLecture = lectures.FirstOrDefault(l =>
                l.LectureDate.Date == date.Date &&
                l.Id != excludeLectureId);

            return SuccessResponse<bool>(
                conflictingLecture != null,
                conflictingLecture != null ? "There is already a lecture scheduled for this day. Only one lecture per day is allowed." : "No conflicts found"
            );
        }
        catch (Exception ex)
        {
            return ErrorResponse<bool>($"Error checking lecture conflict: {ex.Message}");
        }
    }


    public async Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetMyLecturesAsync(string userId, string userRole)
    {
        try
        {
            IEnumerable<string> courseIds = new List<string>();

            if (userRole == "Student")
            {
                var enrollments = await _unitOfWork.StudentEnrollments.GetAllAsync();
                courseIds = enrollments.Where(e => e.StudentId == userId).Select(e => e.CourseId).ToList();
            }
            else if (userRole == "Instructor")
            {
                var enrollments = await _unitOfWork.InstructorEnrollments.GetAllAsync();
                courseIds = enrollments.Where(e => e.InstructorId == userId && e.Status == Domain.Enums.ApplicationStatus.Approved)
                                       .Select(e => e.CourseId).ToList();
            }
            else if (userRole == "Admin")
            {
                 // Admin sees all? Or nothing? Let's assume nothing for "My Lectures" or maybe all. 
                 // For now, let's return empty or handle as needed. 
                 // User request implies Student and Instructor.
                 return SuccessResponse<IEnumerable<GetLectureDTO>>(new List<GetLectureDTO>(), "Admins can view all lectures via Course management.");
            }

            if (!courseIds.Any())
            {
                return SuccessResponse<IEnumerable<GetLectureDTO>>(new List<GetLectureDTO>(), "No enrolled courses found.");
            }

            var lectures = await _unitOfWork.Lectures.GetLecturesByCourseIdsAsync(courseIds);
            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);

            return SuccessResponse<IEnumerable<GetLectureDTO>>(lectureDTOs, "My lectures retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<IEnumerable<GetLectureDTO>>($"Error retrieving my lectures: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<bool>> LaunchLectureAsync(string lectureId, string zoomLink, string userId, string userRole)
    {
        try
        {
            // 1. Validate User (Instructor or Admin)
            if (userRole != "Admin" && userRole != "Instructor")
            {
                return ErrorResponse<bool>("Only instructors or admins can launch lectures.");
            }

            // 2. Get Lecture
            var lecture = await _unitOfWork.Lectures.GetQueryable()
                .Include(l => l.Course)
                .Include(l => l.Course.Students).ThenInclude(se => se.Student)
                .Include(l => l.Instructor)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return ErrorResponse<bool>("Lecture not found.");
            }

            // 3. Verify Instructor Ownership
            if (userRole == "Instructor" && lecture.InstructorId != userId)
            {
                return ErrorResponse<bool>("You can only launch your own lectures.");
            }

            // 4. Update Zoom Link
            lecture.ZoomLink = zoomLink;
            await _unitOfWork.Lectures.UpdateAsync(lecture);
            await _unitOfWork.SaveChangesAsync();

            // 5. Notify Students
            foreach (var enrollment in lecture.Course.Students)
            {
                if (enrollment.Student != null)
                {
                    var subject = $"Lecture Started: {lecture.Course.Name}";
                    var body = $@"
                        <h2>Lecture Started</h2>
                        <p>Hello {enrollment.Student.UserName},</p>
                        <p>The lecture for <strong>{lecture.Course.Name}</strong> has started.</p>
                        <p><strong>Topic:</strong> {lecture.Title}</p>
                        <p><strong>Instructor:</strong> {lecture.Instructor?.UserName ?? "Unknown"}</p>
                        <p><a href='{zoomLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Join Lecture</a></p>
                        <p>Or copy this link: {zoomLink}</p>";

                    // Fire and forget email to avoid blocking the response
                    _ = _emailService.SendEmailAsync(enrollment.Student.Email, subject, body);
                }
            }

            return SuccessResponse<bool>(true, "Lecture launched successfully and students notified.");
        }
        catch (Exception ex)
        {
            return ErrorResponse<bool>($"Error launching lecture: {ex.Message}");
        }
    }

    public Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId)
    {
        throw new NotImplementedException();
    }
}