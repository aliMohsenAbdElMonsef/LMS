using Domain.Entities.MainEntities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts
{
    public interface IBaseRepository<TEntity, TId> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> FindByIdAsync(TId id);

        Task CreateAsync(TEntity entity);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        Task UpdateAsync(TEntity entity);

        Task DeleteByEntityAsync(TEntity entity);

        Task DeleteWithIDAsync(TId id);
    }
}
