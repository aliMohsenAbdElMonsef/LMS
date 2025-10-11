using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts
{

    public interface IBaseService<TReadDto, TCreateDto, TUpdateDto> where TReadDto : class where TCreateDto : class where TUpdateDto : class
    {
        IEnumerable<TReadDto> GetAll();
        TReadDto GetById(string id);

        TReadDto Create(TCreateDto dto);
        TReadDto Update(TUpdateDto dto);
        void Delete(string id);
    }

}
