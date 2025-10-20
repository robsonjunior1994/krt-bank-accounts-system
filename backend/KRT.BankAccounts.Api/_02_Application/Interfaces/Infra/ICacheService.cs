namespace KRT.BankAccounts.Api._02_Application.Interfaces.Infra;

public interface ICacheService
{
    /// <summary>
    /// Obtém um valor do cache.
    /// </summary>
    /// <typeparam name="T">Tipo esperado do valor.</typeparam>
    /// <param name="key">Chave do cache.</param>
    /// <returns>O valor do cache ou <c>default</c> se não encontrado.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Armazena um valor no cache com tempo de expiração opcional.
    /// Caso o tempo não seja informado, será utilizado o valor padrão configurado no <see cref="CacheSettings"/>.
    /// </summary>
    /// <typeparam name="T">Tipo do valor a ser armazenado.</typeparam>
    /// <param name="key">Chave do cache.</param>
    /// <param name="value">Valor a ser armazenado.</param>
    /// <param name="expiration">
    /// Tempo de vida do cache (TTL). 
    /// Se nulo, será aplicado o tempo padrão definido na configuração da aplicação.
    /// </param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Remove um valor do cache.
    /// </summary>
    /// <param name="key">Chave do cache a ser removida.</param>
    Task RemoveAsync(string key);
}
