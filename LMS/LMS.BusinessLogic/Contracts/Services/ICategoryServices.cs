using LMS.BusinessLogic.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ICategoryServices : IBaseService<ReadCategoryDTO, CreateCategoryDTO, UpdateCategoryDTO>
    {

    }
}
