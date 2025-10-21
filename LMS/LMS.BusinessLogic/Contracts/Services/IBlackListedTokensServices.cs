using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.BusinessLogic.Contracts.Services
{
    public interface IBlackListedTokensServices
    {
        Task<bool> IsTokenBlackListedAsync(string token);
        Task AddTokenAsync(string token, DateTime expiryDate, string userId);
        Task RemoveExpiredTokensAsync();
    }
}
