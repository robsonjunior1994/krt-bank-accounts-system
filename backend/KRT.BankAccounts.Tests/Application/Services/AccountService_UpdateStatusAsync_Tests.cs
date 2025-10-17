using Moq;
using KRT.BankAccounts.Api._02_Application.Services;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Enums;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._01_Presentation.Helpers;
using Microsoft.Extensions.Options;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;

namespace KRT.BankAccounts.Tests.Application.Services
{
    public class AccountService_UpdateStatusAsync_Tests
    {
        private readonly Mock<IAccountRepository> _repositoryMock;
        private readonly Mock<IMessagePublisher> _publisherMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IOptions<CacheSettings>> _settingsMock;
        private readonly AccountService _service;

        public AccountService_UpdateStatusAsync_Tests()
        {
            _repositoryMock = new Mock<IAccountRepository>();
            _publisherMock = new Mock<IMessagePublisher>();
            _cacheMock = new Mock<ICacheService>();
            _settingsMock = new Mock<IOptions<CacheSettings>>();

            _settingsMock.Setup(s => s.Value).Returns(new CacheSettings());

            _service = new AccountService(
                _repositoryMock.Object,
                _publisherMock.Object,
                _cacheMock.Object,
                _settingsMock.Object
            );
        }

        [Fact(DisplayName = "Deve ativar conta com sucesso")]
        public async Task UpdateStatusAsync_ShouldActivateAccountSuccessfully()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            account.Deactivate(); // deixa inativa para poder ativar

            _repositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);
            _publisherMock.Setup(p => p.PublishAsync("account.activate", It.IsAny<object>())).Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateStatusAsync(account.Id, ativar: true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AccountStatus.Active.ToString(), result.Data.Status);
            _repositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
            _publisherMock.Verify(p => p.PublishAsync("account.activate", It.IsAny<object>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"account_{account.Id}"), Times.Once);
        }

        [Fact(DisplayName = "Deve desativar conta com sucesso")]
        public async Task UpdateStatusAsync_ShouldDeactivateAccountSuccessfully()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");

            _repositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
            _repositoryMock.Setup(r => r.UpdateAsync(account)).Returns(Task.CompletedTask);
            _publisherMock.Setup(p => p.PublishAsync("account.disable", It.IsAny<object>())).Returns(Task.CompletedTask);
            _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateStatusAsync(account.Id, ativar: false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AccountStatus.Inactive.ToString(), result.Data.Status);
            _repositoryMock.Verify(r => r.UpdateAsync(account), Times.Once);
            _publisherMock.Verify(p => p.PublishAsync("account.disable", It.IsAny<object>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"account_{account.Id}"), Times.Once);
        }

        [Fact(DisplayName = "Deve falhar quando conta não for encontrada")]
        public async Task UpdateStatusAsync_ShouldFail_WhenAccountNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Account)null);

            // Act
            var result = await _service.UpdateStatusAsync(99, true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.NOT_FOUND, result.ErrorCode);
            Assert.Equal("Conta não encontrada.", result.ErrorMessage);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact(DisplayName = "Deve falhar quando tentar ativar uma conta já ativa")]
        public async Task UpdateStatusAsync_ShouldFail_WhenActivateAlreadyActive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");

            _repositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);

            // Act
            var result = await _service.UpdateStatusAsync(account.Id, ativar: true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.ErrorCode);
            Assert.Contains("já está ativa", result.ErrorMessage);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact(DisplayName = "Deve falhar quando tentar desativar uma conta já inativa")]
        public async Task UpdateStatusAsync_ShouldFail_WhenDeactivateAlreadyInactive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            account.Deactivate();

            _repositoryMock.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);

            // Act
            var result = await _service.UpdateStatusAsync(account.Id, ativar: false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.ErrorCode);
            Assert.Contains("já está inativa", result.ErrorMessage);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
            _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact(DisplayName = "Deve falhar quando ocorrer erro inesperado no repositório")]
        public async Task UpdateStatusAsync_ShouldFail_WhenExceptionThrown()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            _repositoryMock.Setup(r => r.GetByIdAsync(account.Id))
                           .ThrowsAsync(new Exception("Erro simulado"));

            // Act
            var result = await _service.UpdateStatusAsync(account.Id, ativar: true);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
            Assert.Equal("Ocorreu um erro ao atualizar o status da conta.", result.ErrorMessage);
        }
    }
}
