using LMS.BusinessLogic.DTOs.Assignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IAssignmentServices: IBaseService<ReadAssignmentDTO, CreateAssignmentDTO, UpdateAssignmentDTO>
    {
    }
}
