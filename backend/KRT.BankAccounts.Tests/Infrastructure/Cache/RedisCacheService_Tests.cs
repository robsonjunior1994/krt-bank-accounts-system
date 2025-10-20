using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace KRT.BankAccounts.Tests.Infrastructure.Cache
{
    public class RedisCacheService_Tests
    {
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<IConnectionMultiplexer> _connectionMock;
        private readonly Mock<IOptions<CacheSettings>> _optionsMock;
        private readonly RedisCacheService _service;

        public RedisCacheService_Tests()
        {
            _databaseMock = new Mock<IDatabase>();
            _connectionMock = new Mock<IConnectionMultiplexer>();
            _optionsMock = new Mock<IOptions<CacheSettings>>();

            _optionsMock.Setup(o => o.Value).Returns(new CacheSettings
            {
                DefaultExpirationMinutes = 15
            });

            _connectionMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(_databaseMock.Object);

            _service = new RedisCacheService(_connectionMock.Object, _optionsMock.Object);
        }

        [Fact(DisplayName = "Deve retornar valor desserializado corretamente")]
        public async Task GetAsync_ShouldReturnDeserializedValue_WhenValidJson()
        {
            // Arrange
            var key = "user:1";
            var expected = new AccountTest { Name = "Robson", Age = 30 };
            var json = JsonSerializer.Serialize(expected);

            _databaseMock.Setup(d => d.StringGetAsync(key, CommandFlags.None))
                         .ReturnsAsync(json);

            // Act
            var result = await _service.GetAsync<AccountTest>(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Name, result!.Name);
            Assert.Equal(expected.Age, result.Age);
            _databaseMock.Verify(d => d.StringGetAsync(key, CommandFlags.None), Times.Once);
        }

        [Fact(DisplayName = "Deve retornar null quando valor for nulo ou vazio")]
        public async Task GetAsync_ShouldReturnNull_WhenValueIsEmpty()
        {
            // Arrange
            var key = "empty";
            _databaseMock.Setup(d => d.StringGetAsync(key, CommandFlags.None))
                         .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await _service.GetAsync<AccountTest>(key);

            // Assert
            Assert.Null(result);
            _databaseMock.Verify(d => d.StringGetAsync(key, CommandFlags.None), Times.Once);
        }

        [Fact(DisplayName = "Deve remover chave e retornar null quando valor for corrompido")]
        public async Task GetAsync_ShouldRemoveKey_WhenDeserializationFails()
        {
            // Arrange
            var key = "invalid";
            _databaseMock.Setup(d => d.StringGetAsync(key, CommandFlags.None))
                         .ReturnsAsync("valor_invalido");

            _databaseMock.Setup(d => d.KeyDeleteAsync(key, CommandFlags.None))
                         .ReturnsAsync(true);

            // Act
            var result = await _service.GetAsync<AccountTest>(key);

            // Assert
            Assert.Null(result);
            _databaseMock.Verify(d => d.KeyDeleteAsync(key, CommandFlags.None), Times.Once);
        }

        [Fact(DisplayName = "Deve salvar valor no cache com TTL padrão")]
        public async Task SetAsync_ShouldSaveValueWithDefaultTTL()
        {
            // Arrange
            var key = "user:2";
            var value = new AccountTest { Name = "Junior", Age = 25 };

            _databaseMock.Setup(d => d.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                false, When.Always, CommandFlags.None))
                .ReturnsAsync(true);

            // Act
            await _service.SetAsync(key, value);

            // Assert
            _databaseMock.Verify(d => d.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                TimeSpan.FromMinutes(15),
                false, When.Always, CommandFlags.None),
                Times.Once);
        }

        [Fact(DisplayName = "Deve salvar valor no cache com TTL customizado")]
        public async Task SetAsync_ShouldSaveValueWithCustomTTL()
        {
            // Arrange
            var key = "user:3";
            var value = new AccountTest { Name = "Carlos", Age = 28 };
            var expiration = TimeSpan.FromMinutes(5);

            _databaseMock.Setup(d => d.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                false, When.Always, CommandFlags.None))
                .ReturnsAsync(true);

            // Act
            await _service.SetAsync(key, value, expiration);

            // Assert
            _databaseMock.Verify(d => d.StringSetAsync(
                key,
                It.IsAny<RedisValue>(),
                expiration,
                false, When.Always, CommandFlags.None),
                Times.Once);
        }

        [Fact(DisplayName = "Deve remover chave do cache")]
        public async Task RemoveAsync_ShouldDeleteKey()
        {
            // Arrange
            var key = "user:4";
            _databaseMock.Setup(d => d.KeyDeleteAsync(key, CommandFlags.None))
                         .ReturnsAsync(true);

            // Act
            await _service.RemoveAsync(key);

            // Assert
            _databaseMock.Verify(d => d.KeyDeleteAsync(key, CommandFlags.None), Times.Once);
        }

        private class AccountTest
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }
    }
}
