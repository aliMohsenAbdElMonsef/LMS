using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.DTOs.Responses
{
    public class ServiceResponseDTO<T>: BasicResponseDTO
    {
        public T? Data { get; set; }
    }
}
