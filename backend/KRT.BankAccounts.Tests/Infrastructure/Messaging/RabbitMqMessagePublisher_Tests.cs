using KRT.BankAccounts.Api._04_Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System.Text;

namespace KRT.BankAccounts.Tests.Infrastructure.Messaging
{
    public class RabbitMqMessagePublisher_Tests
    {
        private readonly Mock<IConnection> _connectionMock;
        private readonly Mock<IChannel> _channelMock;
        private readonly Mock<IOptions<RabbitMqSettings>> _optionsMock;
        private readonly RabbitMqMessagePublisher _publisher;
        private readonly RabbitMqSettings _settings;

        public RabbitMqMessagePublisher_Tests()
        {
            _connectionMock = new Mock<IConnection>();
            _channelMock = new Mock<IChannel>();
            _optionsMock = new Mock<IOptions<RabbitMqSettings>>();

            _settings = new RabbitMqSettings
            {
                Host = "localhost",
                Port = 5672,
                Username = "guest",
                Password = "guest",
                Exchange = "test-exchange",
                Queue = "test-queue",
                RoutingKey = "test.route"
            };

            _optionsMock.Setup(x => x.Value).Returns(_settings);

            _publisher = new RabbitMqMessagePublisher(_optionsMock.Object);

            // injeta mocks via reflexão (pois os campos são privados)
            typeof(RabbitMqMessagePublisher)
                .GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_publisher, _connectionMock.Object);

            typeof(RabbitMqMessagePublisher)
                .GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_publisher, _channelMock.Object);
        }

        [Fact(DisplayName = "Deve publicar mensagem com sucesso")]
        public async Task PublishAsync_ShouldPublishMessageSuccessfully()
        {
            // Arrange
            _channelMock.Setup(c => c.IsOpen).Returns(true);
            _channelMock
                .Setup(c => c.BasicPublishAsync<BasicProperties>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            // Act
            await _publisher.PublishAsync("account.created", new { Id = 1, Name = "Robson" });

            // Assert
            _channelMock.Verify(c => c.BasicPublishAsync<BasicProperties>(
                _settings.Exchange,
                "account.created",
                false,
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact(DisplayName = "Deve serializar payload corretamente antes de publicar")]
        public async Task PublishAsync_ShouldSerializePayloadCorrectly()
        {
            // Arrange
            _channelMock.Setup(c => c.IsOpen).Returns(true);

            string? capturedJson = null;

            _channelMock
                .Setup(c => c.BasicPublishAsync<BasicProperties>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Callback((string _, string __, bool ___, BasicProperties ____, ReadOnlyMemory<byte> body, CancellationToken _____) =>
                {
                    capturedJson = Encoding.UTF8.GetString(body.ToArray());
                })
                .Returns(ValueTask.CompletedTask);

            var payload = new { Id = 99, Name = "Alice" };

            // Act
            await _publisher.PublishAsync("account.created", payload);

            // Assert
            Assert.NotNull(capturedJson);
            Assert.Contains("account.created", capturedJson);
            Assert.Contains("Alice", capturedJson);
        }

        [Fact(DisplayName = "Deve chamar DisposeAsync e liberar recursos")]
        public async Task DisposeAsync_ShouldDisposeChannelAndConnection()
        {
            // Arrange
            _channelMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            _connectionMock.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();

            // Act
            await _publisher.DisposeAsync();

            // Assert
            _channelMock.Verify(c => c.DisposeAsync(), Times.Once);
            _connectionMock.Verify(c => c.DisposeAsync(), Times.Once);
        }

        [Fact(DisplayName = "Deve publicar múltiplas mensagens reutilizando canal aberto")]
        public async Task PublishAsync_ShouldReuseOpenChannelForMultipleMessages()
        {
            // Arrange
            _channelMock.Setup(c => c.IsOpen).Returns(true);
            _channelMock.Setup(c => c.BasicPublishAsync<BasicProperties>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            await _publisher.PublishAsync("account.updated", new { Id = 5 });
            await _publisher.PublishAsync("account.deleted", new { Id = 7 });

            // Assert
            _channelMock.Verify(c => c.BasicPublishAsync<BasicProperties>(
                _settings.Exchange,
                It.IsAny<string>(),
                false,
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}
