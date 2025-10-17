using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace KRT.BankAccounts.Api._04_Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly CacheSettings _settings;

        public RedisCacheService(IConnectionMultiplexer connection, IOptions<CacheSettings> options)
        {
            _database = connection.GetDatabase();
            _settings = options.Value;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedValue = await _database.StringGetAsync(key);

            if (cachedValue.IsNullOrEmpty)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(cachedValue!);
            }
            catch
            {
                // Caso o valor no cache esteja corrompido (ou tipo diferente)
                await RemoveAsync(key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var json = JsonSerializer.Serialize(value);

            // Usa o TTL configurado no appsettings caso não seja informado
            var ttl = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            await _database.StringSetAsync(key, json, ttl);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
