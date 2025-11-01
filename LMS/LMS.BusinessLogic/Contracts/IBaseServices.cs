using LMS.BusinessLogic.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts
{

    public interface IBaseService<TReadDto, TCreateDto, TUpdateDto> where TReadDto : class where TCreateDto : class where TUpdateDto : class
    {
        Task<ServiceResponseDTO<IEnumerable<TReadDto>>> GetAllAsync();
        Task<ServiceResponseDTO<TReadDto>> GetByIdAsync(string id);

        Task<ServiceResponseDTO<TReadDto>> CreateAsync(TCreateDto dto);
        Task<ServiceResponseDTO<TReadDto>> UpdateAsync(TUpdateDto dto);
        Task<ServiceResponseDTO<TReadDto>> DeleteAsync(string id);
    }

}
