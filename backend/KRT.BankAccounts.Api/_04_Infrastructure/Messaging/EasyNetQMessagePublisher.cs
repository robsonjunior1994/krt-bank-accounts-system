using EasyNetQ;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace KRT.BankAccounts.Api._04_Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class EasyNetQMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IBus _bus;
        private readonly RabbitMqSettings _settings;

        public EasyNetQMessagePublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var connectionString =
                $"host={_settings.Host};" +
                $"port={_settings.Port};" +
                $"username={_settings.Username};" +
                $"password={_settings.Password};" +
                $"publisherConfirms=true;timeout=10";

            _bus = RabbitHutch.CreateBus(connectionString);
        }

        public async Task PublishAsync(string eventType, object data)
        {
            var message = new
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Payload = data
            };

            await _bus.PubSub.PublishAsync(message);

            Console.WriteLine($"EasyNetQ] Event published: {eventType}");
        }

        public void Dispose()
        {
            _bus.Dispose();
        }
    }
}
