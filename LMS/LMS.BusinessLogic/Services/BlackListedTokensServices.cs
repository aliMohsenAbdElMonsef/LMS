using LMS.BusinessLogic.Contracts.Services;
using LMS.DataAccess.Contracts.Repositories;
using LMS.Entity.Entities.MainEntities;

namespace LMS.BusinessLogic.Services
{
    internal class BlackListedTokensServices : IBlackListedTokensServices
    {
        private readonly IBlackListedTokens _blackListedTokensRepository;
        public BlackListedTokensServices(IBlackListedTokens repository)
        {
            _blackListedTokensRepository = repository;
        }

        public async Task<bool> IsTokenBlackListedAsync(string token)
        {
            var blackToken = await _blackListedTokensRepository
            .GetFirstOrDefaultAsync(x => x.Token == token);

            if (blackToken == null)
                return false;

            if (blackToken.ExpiryDate < DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task AddTokenAsync(string token, DateTime expiryDate, string userId)
        {
            var blackListed = new BlackListedTokens
            {
                Token = token,
                ExpiryDate = expiryDate,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _blackListedTokensRepository.AddAsync(blackListed);
            await _blackListedTokensRepository.SaveChangesAsync();
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var expired = await _blackListedTokensRepository.GetAllAsync(t => t.ExpiryDate < DateTime.UtcNow);
            foreach (var item in expired)
            {
                await _blackListedTokensRepository.DeleteAsync(item);
            }
            await _blackListedTokensRepository.SaveChangesAsync();
        }
    }
}
