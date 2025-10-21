using LMS.BusinessLogic.Contracts.Services;
using LMS.DataAcess.Contracts.Repositories;
using LMS.Entity.Entities.MainEntities;

namespace LMS.BusinessLogic.Services
{
    internal class BlackListedTokensServices : IBlackListedTokensServices
    {
        private readonly IBlackListedTokens _repository;
        public BlackListedTokensServices(IBlackListedTokens repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsTokenBlackListedAsync(string token)
        {
            var result = await _repository.GetFirstOrDefaultAsync(t => t.Token == token);
            return result != null && result.ExpiryDate > DateTime.UtcNow;
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

            await _repository.AddAsync(blackListed);
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var expired = await _repository.GetAllAsync(t => t.ExpiryDate < DateTime.UtcNow);
            foreach (var item in expired)
            {
                await _repository.DeleteAsync(item);
            }
        }
    }
}
