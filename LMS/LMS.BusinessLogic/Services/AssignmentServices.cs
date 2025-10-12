using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS.BusinessLogic.DTOs.Assignment;
using LMS.DataAcess.Contracts;
using Microsoft.AspNetCore.Http;
using LMS.BusinessLogic.Contracts.Services;
namespace LMS.BusinessLogic.Services
{
    internal class AssignmentServices : BaseServices<Assignment, ReadAssignmentDTO, CreateAssignmentDTO, UpdateAssignmentDTO>, IAssignmentServices
    {
        public AssignmentServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        protected override string GetIdFromUpdateDTO(UpdateAssignmentDTO dto) => dto.Id;


        protected override IBaseRepository<Assignment, string> GetRepo() => _unitOfWork.Assignments;


        private void CreateFile(IFormFile file, Assignment assignment)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "assignments");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            assignment.FilePath = Path.Combine("uploads", "assignments", fileName);
        }
        protected override Assignment MapToEntity(CreateAssignmentDTO dto)
        {
            Assignment assignment = new Assignment
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CourseId = dto.CourseId
            };
            // handle file upload
            if (dto.File != null && dto.File.Length > 0)
            {
                CreateFile(dto.File, assignment);
            }
            else
            {
                throw new ArgumentException("File is required");
            }
            return assignment;
        }
        protected override ReadAssignmentDTO MapToReadDTO(Assignment entity)
        {
            ReadAssignmentDTO dto = new ReadAssignmentDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                FilePath = entity.FilePath,
                UploadDate = entity.UploadDate,
                DueDate = entity.DueDate,
                InstructorId = entity.InstructorId,
                InstructorName = entity.Instructor != null ? $"{entity.Instructor.FirstName} {entity.Instructor.LastName}" : null,
                CourseId = entity.CourseId,
                CourseName = entity.Course != null ? entity.Course.Name : null,
                NumberofSubmissions = entity.Students?.Count(sa => sa.Student != null && !sa.Student.IsDeleted) ?? 0
            };
            return dto;
        }

        protected override Assignment UpdateToEntity(UpdateAssignmentDTO dto, Assignment existingEntity)
        {
            existingEntity.Title = dto.Title ?? existingEntity.Title;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.DueDate = dto.DueDate ?? existingEntity.DueDate;
            existingEntity.CourseId = dto.CourseId ?? existingEntity.CourseId;
            existingEntity.InstructorId = dto.InstructorId ?? existingEntity.InstructorId;
            if (dto.File != null && dto.File.Length > 0)
            {
                // delete old file
                if(!string.IsNullOrEmpty(existingEntity.FilePath))
                {
                    string oldpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingEntity.FilePath);
                    if (File.Exists(oldpath))
                    {
                        File.Delete(oldpath);
                    }
                }

                CreateFile(dto.File, existingEntity);
            }
            existingEntity.UploadDate = DateTime.UtcNow;


            return existingEntity;
        }
    }
}
