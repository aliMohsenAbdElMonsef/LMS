using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.MainEntities;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface ICategoryRepository: IBaseRepository<Category, string>
    {
    }
}
