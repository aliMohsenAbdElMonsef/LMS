using AutoMapper;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.MVC.Models.ViewModels.Assignment;

public class AssignmentMapping : Profile
{
    public AssignmentMapping()
    {
        CreateMap<ReadAssignmentDTO, ReadAssignmentResult>();
        CreateMap<AssignmentDetailsDTO, AssignmentDetailsResult>();
        CreateMap<StudentAssignmentDTO, StudentAssignmentResult>()
            .ForMember(dest => dest.StudentAssignmentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusDisplay));
        CreateMap<ReadAssignmentResult, CreateAssignmentDTO>();
        CreateMap<ReadAssignmentResult, UpdateAssignmentDTO>();
        CreateMap<StudentAssignmentResult, SubmitAssignmentDTO>();
        CreateMap<StudentAssignmentResult, GradeAssignmentDTO>()
            .ForMember(dest => dest.StudentAssignmentId, opt => opt.MapFrom(src => src.Id)); 
    }
}