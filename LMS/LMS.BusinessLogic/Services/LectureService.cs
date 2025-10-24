using AutoMapper;
using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.DataAcess.Contracts;
using LMS.DataAcess.Contracts.Repositories;

public class LectureService : ILectureServices
{
    private readonly IUnitOfWork _unitOfWork; 
    private readonly IMapper _mapper;

    public LectureService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GetLectureDTO>> GetAllAsync()
    {
        var lectures = await _unitOfWork.Lectures.GetAllAsync();
        return _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);
    }

    public async Task<GetLectureDTO> GetByIdAsync(string id)
    {
        var lecture = await _unitOfWork.Lectures.FindByIdAsync(id);
        return _mapper.Map<GetLectureDTO>(lecture);
    }

    public async Task<GetLectureDTO> CreateAsync(CreateLectureDTO dto)
    {
        var entity = _mapper.Map<Lecture>(dto);
        await _unitOfWork.Lectures.CreateAsync(entity);
        return _mapper.Map<GetLectureDTO>(entity);
    }

    public async Task<GetLectureDTO> UpdateAsync(UpdateLectureDTO dto)
    {
        var entity = _mapper.Map<Lecture>(dto);
        await _unitOfWork.Lectures.UpdateAsync(entity);
        return _mapper.Map<GetLectureDTO>(entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _unitOfWork.Lectures.DeleteAsync(id);
    }

    public async Task<IEnumerable<GetLectureDTO>> GetCourseOcturesAsync(string courseId)
    {
        var lectures = await _unitOfWork.Lectures.GetCourseOcturesAsync(courseId);
        return _mapper.Map<IEnumerable<GetLectureDTO>>(lectures);
    }

    public async Task<IEnumerable<GetLectureDTO>> GetInstructorOcturesAsync(string instructorId)
    {
        var lectures = await _unitOfWork.Lectures.GetAllAsync();
        var filtered = lectures.Where(l => l.InstructorId == instructorId);
        return _mapper.Map<IEnumerable<GetLectureDTO>>(filtered);
    }
}
