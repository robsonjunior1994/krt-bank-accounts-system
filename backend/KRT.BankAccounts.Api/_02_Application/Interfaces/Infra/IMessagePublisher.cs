namespace KRT.BankAccounts.Api._02_Application.Interfaces.Infra
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string eventType, object data);
    }
}
