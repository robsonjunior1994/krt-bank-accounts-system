namespace KRT.BankAccounts.Api._02_Application.Interfaces.Infra
{
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publica uma mensagem em um tópico ou fila.
        /// </summary>
        /// <typeparam name="T">Tipo da mensagem (objeto serializável)</typeparam>
        /// <param name="message">Objeto da mensagem</param>
        /// <param name="queueName">Nome da fila/tópico (opcional, se houver padrão)</param>
        Task PublishAsync<T>(T message, string? queueName = null);
    }
}
