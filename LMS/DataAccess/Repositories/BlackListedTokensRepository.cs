using DataAccess.Context;
using LMS.DataAcess.Contracts.Repositories;
using LMS.Entity.Entities.MainEntities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.DataAcess.Repositories
{
    internal class BlackListedTokensRepository :  IBlackListedTokens
    {
        private readonly LMSDbContext _db;
        private readonly DbSet<BlackListedTokens> _dbSet;

        public BlackListedTokensRepository(LMSDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<BlackListedTokens>();
        }

        public async Task AddAsync(BlackListedTokens token)
        {
            await _dbSet.AddAsync(token);
        }

        public async Task<BlackListedTokens?> GetFirstOrDefaultAsync(Expression<Func<BlackListedTokens, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<BlackListedTokens>> GetAllAsync(Expression<Func<BlackListedTokens, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task DeleteAsync(BlackListedTokens token)
        {
            _dbSet.Remove(token);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
