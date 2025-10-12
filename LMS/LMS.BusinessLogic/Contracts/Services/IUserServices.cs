using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IUserServices : IBaseService<ReadUserDTO, CreateUserDTO, UpdateUserDTO>
    {
    }
}
