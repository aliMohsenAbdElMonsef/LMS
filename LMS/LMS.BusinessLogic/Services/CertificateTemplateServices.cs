using Domain.Entities.MainEntities;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.CertificateTemplate;
using LMS.DataAccess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Services
{
    internal class CertificateTemplateServices : BaseServices<CertificateTemplate, ReadCertificateTemplateDTO, CreateCertificateTemplateDTO, UpdateCertificateTemplateDTO>, ICertificateTemplateServices
    {
        public CertificateTemplateServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        protected override string GetIdFromUpdateDTO(UpdateCertificateTemplateDTO dto) => dto.Id;

        protected override IBaseRepository<CertificateTemplate, string> GetRepo() => _unitOfWork.CertificateTemplates;

        protected override CertificateTemplate MapToEntity(CreateCertificateTemplateDTO dto)
        {
            CertificateTemplate certificateTemplate = new CertificateTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                AdminId = dto.AdminId,
                CourseId = dto.CourseId,
                CreatedAt = DateTime.UtcNow,
            };
            return certificateTemplate;
        }

        protected override ReadCertificateTemplateDTO MapToReadDTO(CertificateTemplate entity)
        {
            ReadCertificateTemplateDTO dto = new ReadCertificateTemplateDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description ?? "",
                MessageBody = entity.MessageBody,
                AdminId = entity.AdminId,
                AdminName = entity.Admin.FirstName + " " + entity.Admin.LastName,
                CourseId = entity.CourseId,
                CourseName = entity.Course.Name,
                CreatedAt = entity.CreatedAt
            };
            return dto;
        }

        protected override CertificateTemplate UpdateToEntity(UpdateCertificateTemplateDTO dto, CertificateTemplate existingEntity)
        {
            existingEntity.Title = dto.Title ?? existingEntity.Title;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            return existingEntity;
        }
    }
}
