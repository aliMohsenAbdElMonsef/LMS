using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.BusinessLogic.Services;
using LMS.DataAccess.Contracts;

internal class LectureService : BaseServices<Lecture, GetLectureDTO, CreateLectureDTO, UpdateLectureDTO>, ILectureServices
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public LectureService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
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

            //var conflictCheck = await CheckLectureConflictAsync(dto.CourseId, dto.LectureDate, dto.StartTime, dto.EndTime);
            //if (conflictCheck.Data)
            //{
            //    return ErrorResponse<GetLectureDTO>($"Time conflict detected: {conflictCheck.Message}");
            //}

            var baseResult = await base.CreateAsync(dto);
            return baseResult;
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
                return ErrorResponse<GetLectureDTO>($"Time conflict detected: {conflictCheck.Message}");
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
                return ErrorResponse<IEnumerable<GetLectureDTO>>("You are not authorized to access lectures for this course");
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
                return ErrorResponse<IEnumerable<GetLectureDTO>>("You are not authorized to access lectures for this course");
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
                return ErrorResponse<IEnumerable<GetLectureDTO>>("You are not authorized to access lectures for this course");
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
            var conflictingLecture = lectures.FirstOrDefault(l =>
                l.LectureDate.Date == date.Date &&
                l.Id != excludeLectureId &&
                ((startTime >= l.StartTime && startTime < l.EndTime) ||
                 (endTime > l.StartTime && endTime <= l.EndTime) ||
                 (startTime <= l.StartTime && endTime >= l.EndTime)));

            return SuccessResponse<bool>(
                conflictingLecture != null,
                conflictingLecture != null ? "Lecture time conflict detected" : "No time conflicts found"
            );
        }
        catch (Exception ex)
        {
            return ErrorResponse<bool>($"Error checking lecture conflict: {ex.Message}");
        }
    }


    public Task<ServiceResponseDTO<IEnumerable<GetLectureDTO>>> GetInstructorLecturesAsync(string instructorId)
    {
        throw new NotImplementedException();
    }
}