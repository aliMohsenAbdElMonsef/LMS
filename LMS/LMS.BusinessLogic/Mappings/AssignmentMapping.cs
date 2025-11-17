using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Entities.RelationTables;
using Domain.Enums;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.Entity.Enums;

namespace LMS.BusinessLogic.Mappings
{
    internal class AssignmentMapping : Profile
    {
        public AssignmentMapping()
        {
            // Entity to DTO Mappings

            // Assignment to ReadAssignmentDTO
            CreateMap<Assignment, ReadAssignmentDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.UserName))
                .ForMember(dest => dest.SubmissionsCount, opt => opt.MapFrom(src => src.Students.Count));

            // Assignment to AssignmentDetailsDTO
            CreateMap<Assignment, AssignmentDetailsDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.UserName))
                .ForMember(dest => dest.SubmissionsCount, opt => opt.MapFrom(src => src.Students.Count))
                .ForMember(dest => dest.StudentSubmissions, opt => opt.MapFrom(src => src.Students));

            // StudentAssignment to StudentAssignmentDTO (UPDATED with enum)
            CreateMap<StudentAssignment, StudentAssignmentDTO>()
               .ForMember(dest => dest.StudentName,
                   opt => opt.MapFrom(src => src.Student != null ? src.Student.UserName : string.Empty))
               .ForMember(dest => dest.AssignmentTitle,
                   opt => opt.MapFrom(src => src.Assignment != null ? src.Assignment.Title : string.Empty))
               .ForMember(dest => dest.CourseName,
                   opt => opt.MapFrom(src => src.Assignment != null && src.Assignment.Course != null
                       ? src.Assignment.Course.Name : string.Empty))
               .ForMember(dest => dest.DueDate,
                   opt => opt.MapFrom(src => src.Assignment != null ? src.Assignment.DueDate : DateTime.MinValue))
               .ForMember(dest => dest.Status,
                   opt => opt.MapFrom(src => src.Status));

            // DTO to Entity Mappings

            // CreateAssignmentDTO to Assignment
            CreateMap<CreateAssignmentDTO, Assignment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.UploadDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Students, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore());

            // UpdateAssignmentDTO to Assignment
            CreateMap<UpdateAssignmentDTO, Assignment>()
                .ForMember(dest => dest.Students, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore());

            // SubmitAssignmentDTO to StudentAssignment (UPDATED with enum)
            CreateMap<SubmitAssignmentDTO, StudentAssignment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => AssignmentStatus.PendingGrading)) // NEW: Set default status
                .ForMember(dest => dest.Grade, opt => opt.MapFrom(_ => (double?)null))
                .ForMember(dest => dest.GradedAt, opt => opt.MapFrom(_ => (DateTime?)null))
                .ForMember(dest => dest.Feedback, opt => opt.MapFrom(_ => (string?)null))
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Assignment, opt => opt.Ignore());

            CreateMap<StudentAssignmentDTO, StudentAssignment>()
                .ForMember(dest => dest.Student, opt => opt.Ignore())
                .ForMember(dest => dest.Assignment, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
            CreateMap<GradeAssignmentDTO, StudentAssignment>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.StudentAssignmentId))
    .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => AssignmentStatus.Graded))
    .ForMember(dest => dest.GradedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
    .ForMember(dest => dest.Student, opt => opt.Ignore())
    .ForMember(dest => dest.Assignment, opt => opt.Ignore())
    .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
    .ForMember(dest => dest.FilePath, opt => opt.Ignore());
        }
    }
}