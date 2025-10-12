using LMS.BusinessLogic.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IQuizServices: IBaseService<ReadQuizDTO, CreateQuizDTO, UpdateQuizDTO>
    {
    }
}
