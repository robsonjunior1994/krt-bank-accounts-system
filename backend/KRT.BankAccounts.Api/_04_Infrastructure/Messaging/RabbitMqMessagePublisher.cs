using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace KRT.BankAccounts.Api._04_Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqMessagePublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// Garante que a conexão e o canal sejam criados apenas uma vez, de forma assíncrona.
        /// </summary>
        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _channel != null && _channel.IsOpen)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Cria exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.Exchange,
                type: ExchangeType.Topic,
                durable: true
            );

            // fila e bind apenas em ambiente de dev/teste
            // Só quem deve conhecer a fila é o consumidor
            if (!string.IsNullOrEmpty(_settings.Queue))
            {
                await _channel.QueueDeclareAsync(
                    queue: _settings.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                await _channel.QueueBindAsync(
                    queue: _settings.Queue,
                    exchange: _settings.Exchange,
                    routingKey: _settings.RoutingKey ?? string.Empty
                );
            }

            Console.WriteLine($"[RabbitMQ] Connected to {_settings.Host}:{_settings.Port} - Exchange: {_settings.Exchange}");
        }

        public async Task PublishAsync(string eventType, object data)
        {
            await EnsureConnectionAsync();

            var message = JsonSerializer.Serialize(new
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Payload = data
            });

            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel!.BasicPublishAsync<BasicProperties>(
                exchange: _settings.Exchange,
                routingKey: eventType,
                mandatory: false,
                basicProperties: props,
                body: body
            );

            Console.WriteLine($"[RABBITMQ] Event published → {eventType}");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
                await _channel.DisposeAsync();
            if (_connection != null)
                await _connection.DisposeAsync();
        }
    }
}
