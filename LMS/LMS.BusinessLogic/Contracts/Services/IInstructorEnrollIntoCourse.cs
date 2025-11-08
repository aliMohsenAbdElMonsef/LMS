using LMS.BusinessLogic.DTOs.Enrollment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IInstructorEnrollIntoCourse: IEnrollIntoCourseServices<ReadEnrollIntoCourseDTO,RequestEnrollIntoCourseDTO,UpdateEnrollIntoCourseDTO>
    {
    }
}
