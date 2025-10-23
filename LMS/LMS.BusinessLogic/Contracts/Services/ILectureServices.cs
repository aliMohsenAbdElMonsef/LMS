using LMS.BusinessLogic.DTOs.Lecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ILectureServices: IBaseService<ReadLectureDTO, CreateLectureDTO, UpdateLectureDTO>
    {
    }
}
