using DataAccess.Context;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.DataAccess.Repositories
{
    internal class BaseRepository<TEntity, TId> : IBaseRepository<TEntity, TId> where TEntity : class
    {
        protected readonly LMSDbContext _db;
        protected readonly DbSet<TEntity> _set;

        public BaseRepository(LMSDbContext db)
        {
            _db = db;
            _set = db.Set<TEntity>();
        }

        public virtual async Task CreateAsync(TEntity entity)
        {
            await _set.AddAsync(entity);
        }
        public virtual async Task DeleteWithIDAsync(TId id)
        {
            var entity = await FindByIdAsync(id);
            if (entity == null)
                throw new Exception($"Entity with id {id} not exist.");
            _set.Remove(entity);
        }
        public virtual async Task DeleteByEntityAsync(TEntity entity)
        {
            if (entity == null)
                throw new Exception($"Entity not exist.");
            _set.Remove(entity);
        }

        public virtual async Task<TEntity?> FindByIdAsync(TId id)
        {
            if (id == null) return null;

            var entity = await _set.FindAsync(new object[] { id }!);
            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _set.ToListAsync();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            _set.Update(entity);
            return Task.CompletedTask;
        }
        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _set.FirstOrDefaultAsync(predicate);
        }
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _set.Where(predicate).ToListAsync();
        }

        
    }
}
