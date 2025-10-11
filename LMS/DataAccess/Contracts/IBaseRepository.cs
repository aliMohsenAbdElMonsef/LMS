using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAcess.Contracts
{
    public interface IBaseRepository <TEntity,TId> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        TEntity? FindByID(TId id);

        void Create(TEntity entity);

        void Update(TEntity entity);

        void Delete(TId id);
    }
}
