using Application.DTOs.SkillDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface ISkillServices: IBaseService<ReadSkillDTO, CreateSkillDTO, UpdateSkillDTO>
    {
    }
}
