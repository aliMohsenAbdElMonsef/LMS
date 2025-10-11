using LMS.BusinessLogic.DTOs.CertificateTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICertificateTemplateServices: IBaseService<ReadCertificateTemplateDTO, CreateCertificateTemplateDTO, UpdateCertificateTemplateDTO>
    {
    }
}
