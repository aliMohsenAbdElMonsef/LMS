using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Repositories
{
    internal class BaseRepository<TEntity, TId> : Contracts.IBaseRepository<TEntity, TId> where TEntity : class
    {
        protected readonly LMSDbContext _db;
        protected readonly DbSet<TEntity> _set;
        public BaseRepository(LMSDbContext db)
        {
            _db = db;
            _set = db.Set<TEntity>();
        }

        public void Create(TEntity entity)
        {
            _set.Add(entity);
        }

        public void Delete(TId id)
        {
            TEntity? entity = FindByID(id);
            if (entity != null)
            {
                _set.Remove(entity);
            }
        }

        public TEntity? FindByID(TId id)
        {
            return _set.Find(id);
        }

        public IQueryable<TEntity> GetAll()
        {
            return _set.AsQueryable();
        }

       

        public void Update(TEntity entity)
        {
            _set.Update(entity);
        }
    }
}
