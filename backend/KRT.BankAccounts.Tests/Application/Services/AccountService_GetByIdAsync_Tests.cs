using Moq;
using KRT.BankAccounts.Api._02_Application.Services;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._01_Presentation.DTOs.Response;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using Microsoft.Extensions.Options;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;

namespace KRT.BankAccounts.Tests.Application.Services
{
    public class AccountService_GetByIdAsync_Tests
    {
        private readonly Mock<IAccountRepository> _repositoryMock;
        private readonly Mock<IMessagePublisher> _publisherMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IOptions<CacheSettings>> _settingsMock;
        private readonly AccountService _service;

        public AccountService_GetByIdAsync_Tests()
        {
            _repositoryMock = new Mock<IAccountRepository>();
            _publisherMock = new Mock<IMessagePublisher>();
            _cacheMock = new Mock<ICacheService>();
            _settingsMock = new Mock<IOptions<CacheSettings>>();

            _settingsMock.Setup(s => s.Value).Returns(new CacheSettings
            {
                DefaultExpirationMinutes = 60
            });

            _service = new AccountService(
                _repositoryMock.Object,
                _publisherMock.Object,
                _cacheMock.Object,
                _settingsMock.Object
            );
        }

        [Fact(DisplayName = "Deve retornar conta do cache com sucesso")]
        public async Task GetByIdAsync_ShouldReturnFromCache_WhenCached()
        {
            // Arrange
            var id = 1;
            var cachedResponse = new AccountResponse
            {
                Id = id,
                Name = "Robson Junior",
                Cpf = "12345678901",
                Status = "Active"
            };

            _cacheMock.Setup(c => c.GetAsync<AccountResponse>($"account_{id}"))
                      .ReturnsAsync(cachedResponse);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Robson Junior", result.Data.Name);
            Assert.Equal("12345678901", result.Data.Cpf);
            Assert.Null(result.ErrorCode);

            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<AccountResponse>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact(DisplayName = "Deve buscar do banco e salvar no cache quando não estiver em cache")]
        public async Task GetByIdAsync_ShouldFetchFromDatabase_WhenNotCached()
        {
            // Arrange
            var id = 2;
            var account = new Account("Maria Silva", "98765432100");

            _cacheMock.Setup(c => c.GetAsync<AccountResponse>($"account_{id}"))
                      .ReturnsAsync((AccountResponse)null);

            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync(account);

            _cacheMock.Setup(c => c.SetAsync($"account_{id}", It.IsAny<AccountResponse>(), It.IsAny<TimeSpan>()))
                      .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Maria Silva", result.Data.Name);
            Assert.Equal("98765432100", result.Data.Cpf);

            _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
            _cacheMock.Verify(c => c.SetAsync($"account_{id}", It.IsAny<AccountResponse>(), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact(DisplayName = "Deve retornar erro quando conta não for encontrada")]
        public async Task GetByIdAsync_ShouldFail_WhenAccountNotFound()
        {
            // Arrange
            var id = 3;

            _cacheMock.Setup(c => c.GetAsync<AccountResponse>($"account_{id}"))
                      .ReturnsAsync((AccountResponse)null);

            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync((Account)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.NOT_FOUND, result.ErrorCode);
            Assert.Equal("Conta não encontrada.", result.ErrorMessage);

            _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<AccountResponse>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact(DisplayName = "Deve retornar erro de banco de dados quando ocorrer exceção")]
        public async Task GetByIdAsync_ShouldFail_WhenExceptionThrown()
        {
            // Arrange
            var id = 4;

            _cacheMock.Setup(c => c.GetAsync<AccountResponse>($"account_{id}"))
                      .ThrowsAsync(new Exception("Erro simulado no Redis"));

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
            Assert.Equal("Ocorreu um erro ao buscar a conta.", result.ErrorMessage);
        }

        [Fact(DisplayName = "Deve definir TTL corretamente ao salvar no cache")]
        public async Task GetByIdAsync_ShouldSetCorrectTTL_WhenSavingToCache()
        {
            // Arrange
            var id = 5;
            var account = new Account("João da Silva", "11122233344");

            _cacheMock.Setup(c => c.GetAsync<AccountResponse>($"account_{id}"))
                      .ReturnsAsync((AccountResponse)null);

            _repositoryMock.Setup(r => r.GetByIdAsync(id))
                           .ReturnsAsync(account);

            TimeSpan? capturedTtl = null;

            _cacheMock.Setup(c => c.SetAsync($"account_{id}", It.IsAny<AccountResponse>(), It.IsAny<TimeSpan?>()))
                       .Callback<string, AccountResponse, TimeSpan?>((key, value, ttl) => capturedTtl = ttl)
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(TimeSpan.FromMinutes(60), capturedTtl!.Value);
        }
    }
}
