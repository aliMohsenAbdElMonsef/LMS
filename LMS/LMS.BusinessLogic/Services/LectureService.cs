using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;

internal class LectureService : BaseServices<Lecture, GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>, ILectureServices
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;

    public LectureService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IFileService fileService) : base(unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _fileService = fileService;
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

        if (userRole == "Instructor" && lecture.InstructorId == userId)
        {
            var enrollment = await _unitOfWork.InstructorEnrollments.GetByInstructorAndCourseAsync(userId, lecture.CourseId);
            return enrollment != null && !enrollment.IsDeleted && enrollment.Status == Domain.Enums.ApplicationStatus.Approved;
        }
        if (userRole == "Student")
        {
            var enrollment = await _unitOfWork.StudentEnrollments.GetFirstOrDefaultAsync(userId, lecture.CourseId);
            return enrollment != null && !enrollment.IsDeleted && enrollment.Status == Domain.Enums.ApplicationStatus.Approved;
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
            return enrollment != null && !enrollment.IsDeleted && enrollment.Status == Domain.Enums.ApplicationStatus.Approved;
        }

        if (userRole == "Student")
        {
            var enrollment = await _unitOfWork.StudentEnrollments.GetFirstOrDefaultAsync(userId, courseId);
            return enrollment != null && !enrollment.IsDeleted && enrollment.Status == Domain.Enums.ApplicationStatus.Approved;

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


            var existingLectures = await _unitOfWork.Lectures.GetCourseLecturesAsync(dto.CourseId);
            entity.LectureNumber = existingLectures.Count() + 1;

            if (dto.NewRecordingFile != null)
            {
                var recordingUpload = await _fileService.SaveLectureRecordingAsync(dto.NewRecordingFile);
                if (!recordingUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(recordingUpload.Message);
                }
                entity.RecordingPath = recordingUpload.FileName;
            }

            if (dto.NewMaterialsFile != null)
            {
                var materialsUpload = await _fileService.SaveLectureMaterialAsync(dto.NewMaterialsFile);
                if (!materialsUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(materialsUpload.Message);
                }
                entity.MaterialsPath = materialsUpload.FileName;
            }

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

            var conflictCheck = await CheckLectureConflictAsync(existingLecture.CourseId, dateToCheck, startTimeToCheck, endTimeToCheck, dto.Id);

            if (conflictCheck.Data)
            {
                return ErrorResponse<GetLectureDTO>(conflictCheck.Message);
            }

            if (dto.NewRecordingFile != null)
            {
                if (!string.IsNullOrEmpty(existingLecture.RecordingPath))
                {
                    await _fileService.DeleteLectureRecordingAsync(existingLecture.RecordingPath);
                }
                var recordingUpload = await _fileService.SaveLectureRecordingAsync(dto.NewRecordingFile);
                if (!recordingUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(recordingUpload.Message);
                }
                dto.RecordingPath = recordingUpload.FileName;
            }

            if (dto.NewMaterialsFile != null)
            {
                if (!string.IsNullOrEmpty(existingLecture.MaterialsPath))
                {
                    await _fileService.DeleteLectureMaterialAsync(existingLecture.MaterialsPath);
                }
                var materialsUpload = await _fileService.SaveLectureMaterialAsync(dto.NewMaterialsFile);
                if (!materialsUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(materialsUpload.Message);
                }
                dto.MaterialsPath = materialsUpload.FileName;
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

            if (userRole == "Student")
            {
                await PopulateAttendanceStatus(lectureDTOs, userId);
            }

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
            if(! await CanAccessCourseLectures(instructorId, userId, userRole))
            {
                return ErrorResponse<IEnumerable<GetLectureDTO>>("Access denied. You need an approved enrollment to view this course's lecture schedule.");
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

            if (userRole == "Student")
            {
                await PopulateAttendanceStatus(lectureDTOs, userId);
            }

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
            
            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(todayLectures);

            if (userRole == "Student")
            {
                await PopulateAttendanceStatus(lectureDTOs, userId);
            }

            return SuccessResponse<IEnumerable<GetLectureDTO>>(
                lectureDTOs,
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
                courseIds = enrollments.Where(e => e.StudentId == userId &&!e.IsDeleted &&e.Status == Domain.Enums.ApplicationStatus.Approved).Select(e => e.CourseId).ToList();
            }
            else if (userRole == "Instructor")
            {
                var enrollments = await _unitOfWork.InstructorEnrollments.GetAllAsync();
                courseIds = enrollments.Where(e => e.InstructorId == userId&&!e.IsDeleted && e.Status == Domain.Enums.ApplicationStatus.Approved)
                                       .Select(e => e.CourseId).ToList();
            }
            else if (userRole == "Admin")
            {
                 return SuccessResponse<IEnumerable<GetLectureDTO>>(new List<GetLectureDTO>(), "Admins can view all lectures via Course management.");
            }

            if (!courseIds.Any())
            {
                return SuccessResponse<IEnumerable<GetLectureDTO>>(new List<GetLectureDTO>(), "No enrolled courses found.");
            }

            var lectures = await _unitOfWork.Lectures.GetLecturesByCourseIdsAsync(courseIds);
            var lectureDTOs = _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);

            if (userRole == "Student")
            {
                await PopulateAttendanceStatus(lectureDTOs, userId);
            }

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

            if (userRole != "Admin" && userRole != "Instructor")
            {
                return ErrorResponse<bool>("Only instructors or admins can launch lectures.");
            }


            var lecture = await _unitOfWork.Lectures.GetQueryable()
                .Include(l => l.Course)
                .Include(l => l.Course.Students).ThenInclude(se => se.Student)
                .Include(l => l.Instructor)
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return ErrorResponse<bool>("Lecture not found.");
            }


            if (userRole == "Instructor" && lecture.InstructorId != userId)
            {
                return ErrorResponse<bool>("You can only launch your own lectures.");
            }


            lecture.ZoomLink = zoomLink;
            await _unitOfWork.Lectures.UpdateAsync(lecture);
            await _unitOfWork.SaveChangesAsync();


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

    public async Task<ServiceResponseDTO<bool>> MarkAsAttendedAsync(string lectureId, string studentId)
    {
        try
        {
            var lecture = await _unitOfWork.Lectures.FindByIdAsync(lectureId);
            if (lecture == null)
                return ErrorResponse<bool>("Lecture not found");

            var isEnrolled = await _unitOfWork.StudentEnrollments.GetQueryable()
                .AnyAsync(se => se.StudentId == studentId && se.CourseId == lecture.CourseId && se.Status == Domain.Enums.ApplicationStatus.Approved && !se.IsDeleted);

            if (!isEnrolled)
                return ErrorResponse<bool>("Student is not enrolled in this course");

            var studentLecture = await _unitOfWork.GetQueryable<StudentLecture>()
                .FirstOrDefaultAsync(sl => sl.LectureId == lectureId && sl.StudentId == studentId);

            if (studentLecture == null)
            {
                studentLecture = new StudentLecture
                {
                    LectureId = lectureId,
                    StudentId = studentId,
                    IsAttended = true,
                    AttendanceDate = DateTime.UtcNow
                };
                await _unitOfWork.GetQueryable<StudentLecture>().AddAsync(studentLecture);
            }
            else
            {
                if (!studentLecture.IsAttended)
                {
                    studentLecture.IsAttended = true;
                    studentLecture.AttendanceDate = DateTime.UtcNow;
                    _unitOfWork.GetQueryable<StudentLecture>().Update(studentLecture);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return SuccessResponse(true, "Attendance marked successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<bool>($"Error marking attendance: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<AttendanceStatisticsDTO>> GetAttendanceStatisticsAsync(string studentId)
    {
        try
        {
            var enrolledCourseIds = await _unitOfWork.StudentEnrollments.GetQueryable()
                .Where(se => se.StudentId == studentId && se.Status == Domain.Enums.ApplicationStatus.Approved && !se.IsDeleted)
                .Select(se => se.CourseId)
                .ToListAsync();

            if (!enrolledCourseIds.Any())
            {
                return SuccessResponse(new AttendanceStatisticsDTO { TotalLectures = 0, AttendedLectures = 0, AttendancePercentage = 0.0 }, "No enrollments found");
            }

            var totalLectures = await _unitOfWork.Lectures.GetQueryable()
                .CountAsync(l => enrolledCourseIds.Contains(l.CourseId) && !l.IsDeleted);

            var attendedLectures = await _unitOfWork.GetQueryable<StudentLecture>()
                                        .Join(_unitOfWork.Lectures.GetQueryable(),
                                            sl => sl.LectureId,
                                            l => l.Id,
                                            (sl, l) => new { StudentLecture = sl, Lecture = l })
                                        .CountAsync(x => x.StudentLecture.StudentId == studentId 
                                            && x.StudentLecture.IsAttended 
                                            && !x.StudentLecture.IsDeleted 
                                            && !x.Lecture.IsDeleted);

            double percentage = totalLectures > 0 ? (double)attendedLectures / totalLectures * 100 : 0;

            return SuccessResponse(new AttendanceStatisticsDTO 
            { 
                TotalLectures = totalLectures, 
                AttendedLectures = attendedLectures, 
                AttendancePercentage = percentage 
            }, "Attendance statistics retrieved");
        }
        catch (Exception ex)
        {
            return ErrorResponse<AttendanceStatisticsDTO>($"Error retrieving statistics: {ex.Message}");
        }
    }

    public async Task<ServiceResponseDTO<AttendanceStatisticsDTO>> GetCourseAttendanceStatisticsAsync(string courseId, string studentId)
    {
        try
        {
            var isEnrolled = await _unitOfWork.StudentEnrollments.GetQueryable()
                .AnyAsync(se => se.StudentId == studentId && se.CourseId == courseId && se.Status == Domain.Enums.ApplicationStatus.Approved && !se.IsDeleted);

            if (!isEnrolled)
            {
                return ErrorResponse<AttendanceStatisticsDTO>("Student is not enrolled in this course");
            }

            var totalLectures = await _unitOfWork.Lectures.GetQueryable()
                .CountAsync(l => l.CourseId == courseId && !l.IsDeleted);

            var attendedLectures = await _unitOfWork.GetQueryable<StudentLecture>()
                .Include(sl => sl.Lecture)
                .CountAsync(sl => sl.StudentId == studentId && sl.Lecture.CourseId == courseId && sl.IsAttended && !sl.IsDeleted);

            double percentage = totalLectures > 0 ? (double)attendedLectures / totalLectures * 100 : 0;

            return SuccessResponse(new AttendanceStatisticsDTO 
            { 
                TotalLectures = totalLectures, 
                AttendedLectures = attendedLectures, 
                AttendancePercentage = percentage 
            }, "Course attendance statistics retrieved");
        }
        catch (Exception ex)
        {
            return ErrorResponse<AttendanceStatisticsDTO>($"Error retrieving statistics: {ex.Message}");
        }
    }

    public Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId)
    {
        throw new NotImplementedException();
    }


    private async Task PopulateAttendanceStatus(IEnumerable<GetLectureDTO> lectures, string studentId)
    {
        var lectureIds = lectures.Select(l => l.Id).ToList();
        var studentLectures = await _unitOfWork.GetQueryable<StudentLecture>()
            .Where(sl => sl.StudentId == studentId && lectureIds.Contains(sl.LectureId) && !sl.IsDeleted)
            .ToListAsync();

        var now = DateTime.Now;

        foreach (var lecture in lectures)
        {
            var studentLecture = studentLectures.FirstOrDefault(sl => sl.LectureId == lecture.Id);
            var lectureEnd = lecture.LectureDate.Date + lecture.EndTime;
            var lectureStart = lecture.LectureDate.Date + lecture.StartTime;

            if (studentLecture != null && studentLecture.IsAttended)
            {
                lecture.AttendanceStatus = "Present";
            }
            else if (now > lectureEnd)
            {
                lecture.AttendanceStatus = "Absent";
            }
            else if (now >= lectureStart && now <= lectureEnd)
            {
                lecture.AttendanceStatus = "Ongoing";
            }
            else
            {
                lecture.AttendanceStatus = "Upcoming";
            }
        }
    }
    public async Task<ServiceResponseDTO<GetLectureDTO>> UploadLectureContentAsync(string lectureId, IFormFile? recording, IFormFile? materials, string userId, string userRole)
    {
        try
        {
            if (!await CanManageLecture(lectureId, userId, userRole))
            {
                return ErrorResponse<GetLectureDTO>("You are not authorized to upload content for this lecture");
            }

            var lecture = await _unitOfWork.Lectures.FindByIdAsync(lectureId);
            if (lecture == null)
            {
                return ErrorResponse<GetLectureDTO>("Lecture not found");
            }

            if (recording != null)
            {
                if (!string.IsNullOrEmpty(lecture.RecordingPath))
                {
                    await _fileService.DeleteLectureRecordingAsync(lecture.RecordingPath);
                }
                var recordingUpload = await _fileService.SaveLectureRecordingAsync(recording);
                if (!recordingUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(recordingUpload.Message);
                }
                lecture.RecordingPath = recordingUpload.FileName;
            }

            if (materials != null)
            {
                if (!string.IsNullOrEmpty(lecture.MaterialsPath))
                {
                    await _fileService.DeleteLectureMaterialAsync(lecture.MaterialsPath);
                }
                var materialsUpload = await _fileService.SaveLectureMaterialAsync(materials);
                if (!materialsUpload.Success)
                {
                    return ErrorResponse<GetLectureDTO>(materialsUpload.Message);
                }
                lecture.MaterialsPath = materialsUpload.FileName;
            }

            await _unitOfWork.Lectures.UpdateAsync(lecture);
            await _unitOfWork.SaveChangesAsync();

            var readDto = _mapper.Map<GetLectureDTO>(lecture);
            return SuccessResponse(readDto, "Lecture content uploaded successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse<GetLectureDTO>($"Error uploading lecture content: {ex.Message}");
        }
    }
}