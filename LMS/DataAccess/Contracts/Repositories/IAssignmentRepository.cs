using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts.Repositories
{
    public interface IAssignmentRepository:IBaseRepository<Assignment,string>
    {
    }
}
