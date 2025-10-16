namespace KRT.BankAccounts.Api._02_Application.Interfaces.Infra
{
    public interface ICacheService
    {
        /// <summary>
        /// Obtém um valor do cache.
        /// </summary>
        /// <typeparam name="T">Tipo esperado</typeparam>
        /// <param name="key">Chave do cache</param>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Define um valor no cache com tempo de expiração.
        /// </summary>
        /// <typeparam name="T">Tipo do valor</typeparam>
        /// <param name="key">Chave</param>
        /// <param name="value">Valor</param>
        /// <param name="expiration">Tempo de vida do cache</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Remove um valor do cache.
        /// </summary>
        Task RemoveAsync(string key);
    }
}
