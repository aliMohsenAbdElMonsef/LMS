using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    internal class BaseRepo<TEntity> : IBaseRepo<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<TEntity> _set;
        public BaseRepo(ApplicationDbContext db)
        {
            _db = db;
            _set = db.Set<TEntity>();
        }

        public void Create(TEntity entity)
        {
            _set.Add(entity);
        }

        public void Delete(int id)
        {
            TEntity entity = FindByID(id);
            if (entity != null) 
            {
                _set.Remove(entity);
            }
        }

        public TEntity? FindByID(int id)
        {
           return _set.Find(id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _set.AsEnumerable();
        }

        public void Update(TEntity entity)
        {
            _set.Update(entity);
        }
    }
}
