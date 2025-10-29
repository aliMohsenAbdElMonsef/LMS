using LMS.Entity.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LMS.DataAccess.Contracts.Repositories
{
    public interface IBlackListedTokens
    {
        Task AddAsync(BlackListedTokens token);
        Task<BlackListedTokens?> GetFirstOrDefaultAsync(Expression<Func<BlackListedTokens, bool>> predicate);
        Task<IEnumerable<BlackListedTokens>> GetAllAsync(Expression<Func<BlackListedTokens, bool>> predicate);
        Task DeleteAsync(BlackListedTokens token);
        Task SaveChangesAsync();
    }
}
