using KRT.BankAccounts.Api._01_Presentation.DTOs.Request;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._02_Application.Services;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using Microsoft.Extensions.Options;
using Moq;

namespace KRT.BankAccounts.Tests.Application.Services
{
    public class AccountService_UpdateAsync_Tests
    {
        private readonly Mock<IAccountRepository> _repositoryMock;
        private readonly Mock<IMessagePublisher> _publisherMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IOptions<CacheSettings>> _settingsMock;
        private readonly AccountService _service;

        public AccountService_UpdateAsync_Tests()
        {
            _repositoryMock = new Mock<IAccountRepository>();
            _publisherMock = new Mock<IMessagePublisher>();
            _cacheMock = new Mock<ICacheService>();
            _settingsMock = new Mock<IOptions<CacheSettings>>();

            _service = new AccountService(
                _repositoryMock.Object,
                _publisherMock.Object,
                _cacheMock.Object,
                _settingsMock.Object
            );
        }

        [Fact(DisplayName = "Deve atualizar conta com sucesso")]
        public async Task UpdateAsync_ShouldUpdateAccountSuccessfully()
        {
            // Arrange
            var id = 1;
            var account = new Account("Robson", "12345678901");
            var request = new UpdateAccountRequest
            {
                Name = "Robson Atualizado",
                Cpf = "12345678901"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _publisherMock.Setup(p => p.PublishAsync("account.updated", It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Robson Atualizado", result.Data.Name);
            Assert.Equal("12345678901", result.Data.Cpf);

            _repositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"account_{account.Id}"), Times.Once);
            _publisherMock.Verify(p => p.PublishAsync("account.updated", It.IsAny<object>()), Times.Once);
        }

        [Fact(DisplayName = "Deve falhar quando conta não for encontrada")]
        public async Task UpdateAsync_ShouldFail_WhenAccountNotFound()
        {
            // Arrange
            var request = new UpdateAccountRequest { Name = "Teste", Cpf = "99988877766" };
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Account)null);

            // Act
            var result = await _service.UpdateAsync(1, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.NOT_FOUND, result.ErrorCode);
            Assert.Equal("Conta não encontrada.", result.ErrorMessage);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact(DisplayName = "Deve falhar quando CPF já estiver em uso por outra conta")]
        public async Task UpdateAsync_ShouldFail_WhenCpfAlreadyInUseByAnotherAccount()
        {
            // Arrange
            var id = 2;
            var account = new Account("Robson", "11111111111");
            var request = new UpdateAccountRequest
            {
                Name = "Robson",
                Cpf = "99999999999"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf)).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.RESOURCE_ALREADY_EXISTS, result.ErrorCode);
            Assert.Equal("CPF já está em uso por outra conta.", result.ErrorMessage);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact(DisplayName = "Deve permitir atualizar com o mesmo CPF da própria conta")]
        public async Task UpdateAsync_ShouldAllowUpdate_WhenCpfUnchanged()
        {
            // Arrange
            var id = 3;
            var account = new Account("Robson", "55555555555");
            var request = new UpdateAccountRequest
            {
                Name = "Robson Novo",
                Cpf = "55555555555" // mesmo CPF
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Robson Novo", result.Data.Name);
            Assert.Equal("55555555555", result.Data.Cpf);

            _repositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
        }

        [Fact(DisplayName = "Deve falhar quando ocorrer erro inesperado")]
        public async Task UpdateAsync_ShouldFail_WhenExceptionThrown()
        {
            // Arrange
            var id = 4;
            var account = new Account("Robson", "12345678901");
            var request = new UpdateAccountRequest { Name = "Falha", Cpf = "12345678901" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf)).ThrowsAsync(new Exception("Erro de teste"));

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
            Assert.Equal("Ocorreu um erro ao atualizar a conta.", result.ErrorMessage);
        }

        [Fact(DisplayName = "Deve limpar cache e publicar evento após atualização bem-sucedida")]
        public async Task UpdateAsync_ShouldClearCacheAndPublishEvent()
        {
            // Arrange
            var id = 5;
            var account = new Account("Robson", "12345678901");
            var request = new UpdateAccountRequest { Name = "Novo Nome", Cpf = "12345678901" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);

            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _publisherMock.Setup(p => p.PublishAsync("account.updated", It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            Assert.True(result.IsSuccess);
            _cacheMock.Verify(c => c.RemoveAsync($"account_{account.Id}"), Times.Once);
            _publisherMock.Verify(p => p.PublishAsync("account.updated", It.IsAny<object>()), Times.Once);
        }
    }
}
