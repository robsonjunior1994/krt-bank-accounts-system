using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;

namespace KRT.BankAccounts.Api._04_Infrastructure.Messaging
{
    public class RabbitMqMessagePublisher : IMessagePublisher
    {
        public Task PublishAsync<T>(T message, string? queueName = null)
        {
            throw new NotImplementedException();
        }
    }
}
