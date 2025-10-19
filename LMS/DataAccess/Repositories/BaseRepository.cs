using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using LMS.DataAcess.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
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

        public virtual async Task DeleteAsync(TId id)
        {
            var entity = await FindByIdAsync(id);
            if (entity == null)
                throw new Exception($"Entity with id '{id}' does not exist.");

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
    }
}
