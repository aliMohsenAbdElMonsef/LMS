using AutoMapper;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.MVC.Models.ViewModels.Assignment;

public class AssignmentMapping : Profile
{
    public AssignmentMapping()
    {
        // DTO to ViewModel
        CreateMap<ReadAssignmentDTO, ReadAssignmentResult>();
        CreateMap<AssignmentDetailsDTO, AssignmentDetailsResult>();
        CreateMap<StudentAssignmentDTO, StudentAssignmentResult>()
            .ForMember(dest => dest.StudentAssignmentId, opt => opt.MapFrom(src => src.Id)); // إضافة mapping

        // ViewModel to DTO
        CreateMap<ReadAssignmentResult, CreateAssignmentDTO>();
        CreateMap<ReadAssignmentResult, UpdateAssignmentDTO>();
        CreateMap<StudentAssignmentResult, SubmitAssignmentDTO>();
        CreateMap<StudentAssignmentResult, GradeAssignmentDTO>()
            .ForMember(dest => dest.StudentAssignmentId, opt => opt.MapFrom(src => src.Id)); // إضافة mapping
    }
}