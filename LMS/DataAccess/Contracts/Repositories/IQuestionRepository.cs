using Domain.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IQuestionRepository: IBaseRepository<Question,string>
    {
    }
}
