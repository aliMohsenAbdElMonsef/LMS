using LMS.BusinessLogic.DTOs.Course;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICourseServices: IBaseService<ReadCourseDTO, CreateCourseDTO, UpdateCourseDTO>
    {
    }
}
