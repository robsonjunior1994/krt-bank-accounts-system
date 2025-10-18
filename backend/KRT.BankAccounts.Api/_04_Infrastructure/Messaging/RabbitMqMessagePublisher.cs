using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace KRT.BankAccounts.Api._04_Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqMessagePublisher : IMessagePublisher
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqMessagePublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declaração do Exchange e Queue
            _channel.ExchangeDeclare(exchange: _settings.Exchange, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: _settings.Queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_settings.Queue, _settings.Exchange, _settings.RoutingKey);
        }

        public Task PublishAsync(string eventType, object data)
        {
            var message = JsonSerializer.Serialize(new
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Payload = data
            });

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _settings.Exchange,
                routingKey: eventType,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[🐇 RABBITMQ] Event published: {eventType}");
            return Task.CompletedTask;
        }
    }
}
