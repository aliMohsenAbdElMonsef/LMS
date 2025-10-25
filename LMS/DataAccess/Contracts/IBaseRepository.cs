using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts
{
    public interface IBaseRepository<TEntity, TId> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> FindByIdAsync(TId id);

        Task CreateAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TId id);
    }
}
