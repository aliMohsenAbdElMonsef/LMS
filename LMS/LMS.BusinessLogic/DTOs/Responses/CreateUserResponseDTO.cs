using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Responses
{
    public class CreateUserResponseDTO: BasicResponseDTO
    {
        public string UserId { get; set; }
    }
}
