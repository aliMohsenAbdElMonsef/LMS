using AutoMapper;
using Domain.Entities.MainEntities;
using Domain.Enums;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Course;
using LMS.BusinessLogic.DTOs.Lecture;
using LMS.BusinessLogic.Services.Helpers;
using LMS.DataAcess.Contracts;
using LMS.Entity.Entities.MainEntities;
using LMS.Entity.Entities.RelationTables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CourseServices : BaseServices<Course, ReadCourseDTO, CreateCourseDTO, UpdateCourseDTO>, ICourseServices
    {
        public CourseServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }

        protected override string GetIdFromUpdateDTO(UpdateCourseDTO dto) => dto.Id;

        protected override IBaseRepository<Course, string> GetRepo() => _unitOfWork.Courses;

        protected override Course MapToEntity(CreateCourseDTO dto)
                {
            return new Course
            {
                Id = Guid.NewGuid().ToString(),
                CourseCode = dto.CourseCode,
                Name = dto.Name,
                Description = dto.Description,
                Credits = dto.Credits,
                Level = dto.Level,
                Language = dto.Language,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DurationWeeks = dto.DurationWeeks,
                DeliveryMode = dto.DeliveryMode,
                Status = dto.Status,
                Price = dto.Price,
                IsFree = dto.IsFree,
                ThumbnailPath = dto.ThumbnailPath,
                AutoIssueCertificates = dto.AutoIssueCertificates,
                MinPerformanceScore = dto.MinPerformanceScore,
                MinAttendancePercentage = dto.MinAttendancePercentage,
                AdminId = dto.AdminId,
                CategoryId = dto.CategoryId,
                CertificateTemplateID = dto.CertificateTemplateID,
                LastUpdate = DateTime.UtcNow
            };
                }

        protected override ReadCourseDTO MapToReadDTO(Course entity)
                    {
            return new ReadCourseDTO
            {
                Id = entity.Id,
                CourseCode = entity.CourseCode,
                Name = entity.Name,
                Description = entity.Description,
                Credits = entity.Credits,
                Level = entity.Level,
                Language = entity.Language,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                DurationWeeks = entity.DurationWeeks,
                DeliveryMode = entity.DeliveryMode,
                Status = entity.Status,
                Price = entity.Price,
                IsFree = entity.IsFree,
                ThumbnailPath = entity.ThumbnailPath,
                AutoIssueCertificates = entity.AutoIssueCertificates,
                MinPerformanceScore = entity.MinPerformanceScore,
                MinAttendancePercentage = entity.MinAttendancePercentage,
                AdminId = entity.AdminId,
                CategoryId = entity.CategoryId,
                CertificateTemplateId = entity.CertificateTemplateID,
                LastUpdate = entity.LastUpdate
            };
        }

        protected override Course UpdateToEntity(UpdateCourseDTO dto, Course existingEntity)
            {
            existingEntity.CourseCode = dto.CourseCode ?? existingEntity.CourseCode;
            existingEntity.Name = dto.Name ?? existingEntity.Name;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.Credits = dto.Credits ?? existingEntity.Credits;
            existingEntity.Level = dto.Level ?? existingEntity.Level;
            existingEntity.Language = dto.Language ?? existingEntity.Language;
            existingEntity.StartDate = dto.StartDate ?? existingEntity.StartDate;
            existingEntity.EndDate = dto.EndDate ?? existingEntity.EndDate;
            existingEntity.DurationWeeks = dto.DurationWeeks ?? existingEntity.DurationWeeks;
            existingEntity.DeliveryMode = dto.DeliveryMode ?? existingEntity.DeliveryMode;
            existingEntity.Status = dto.Status ?? existingEntity.Status;
            existingEntity.Price = dto.Price ?? existingEntity.Price;
            existingEntity.IsFree = dto.IsFree ?? existingEntity.IsFree;
            existingEntity.ThumbnailPath = dto.ThumbnailPath ?? existingEntity.ThumbnailPath;
            existingEntity.AutoIssueCertificates = dto.AutoIssueCertificates ?? existingEntity.AutoIssueCertificates;
            existingEntity.MinPerformanceScore = dto.MinPerformanceScore ?? existingEntity.MinPerformanceScore;
            existingEntity.MinAttendancePercentage = dto.MinAttendancePercentage ?? existingEntity.MinAttendancePercentage;
            existingEntity.CategoryId = dto.CategoryId ?? existingEntity.CategoryId;
            existingEntity.CertificateTemplateID = dto.CertificateTemplateID ?? existingEntity.CertificateTemplateID;
            existingEntity.LastUpdate = DateTime.UtcNow;

            return existingEntity;
        }


    }
}
