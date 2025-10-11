using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.MainEntities;
namespace LMS.DataAcess.Contracts.Repositories
{
    public interface ISkillRepository: IBaseRepository<Skills, string>
    {
    }
}
