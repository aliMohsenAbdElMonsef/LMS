using LMS.BusinessLogic.DTOs.Enrollment;
using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IEnrollmentManagement
    {

        Task<ServiceResponseDTO<List<ReadEnrollIntoCourseDTO>>> GetFilteredEnrollmentsAsync(
            string? status, string? userSearch, string? courseSearch, string? role);
    }
}

