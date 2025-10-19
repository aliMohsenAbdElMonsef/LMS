using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts
{

    public interface IBaseService<TReadDto, TCreateDto, TUpdateDto> where TReadDto : class where TCreateDto : class where TUpdateDto : class
    {
        Task<IEnumerable<TReadDto>> GetAllAsync();
        Task<TReadDto> GetByIdAsync(string id);

        Task<TReadDto> CreateAsync(TCreateDto dto);
        Task<TReadDto> UpdateAsync(TUpdateDto dto);
        Task DeleteAsync(string id);
    }

}
