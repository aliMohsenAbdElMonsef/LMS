using LMS.BusinessLogic.DTOs.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IQuestionServices: IBaseService<ReadQuestionDTO, CreateQuestionDTO, UpdateQuestionDTO>
    {
    }
}
