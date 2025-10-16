using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;

namespace KRT.BankAccounts.Api._04_Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            throw new NotImplementedException();
        }
    }
}
