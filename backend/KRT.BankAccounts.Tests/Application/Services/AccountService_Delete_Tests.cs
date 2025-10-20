using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._02_Application.Services;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using Microsoft.Extensions.Options;
using Moq;

namespace KRT.BankAccounts.Tests.Application.Services;

public class AccountService_Delete_Tests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<IOptions<CacheSettings>> _settingsMock;
    private readonly AccountService _service;

    public AccountService_Delete_Tests()
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

    [Fact(DisplayName = "Deve excluir conta com sucesso")]
    public async Task DeleteAsync_ShouldDeleteAccountSuccessfully()
    {
        // Arrange
        var account = new Account("Robson Junior", "11122233344");

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(account);

        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Account>()))
                       .Returns(Task.CompletedTask);

        _publisherMock.Setup(p => p.PublishAsync("account.deleted", It.IsAny<object>()))
                      .Returns(Task.CompletedTask);

        _cacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);

        _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(account), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync("account.deleted", It.IsAny<object>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync($"account_{account.Id}"), Times.Once);
    }

    [Fact(DisplayName = "Deve retornar erro quando conta não encontrada")]
    public async Task DeleteAsync_ShouldFail_WhenAccountNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync((Account)null);

        // Act
        var result = await _service.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NOT_FOUND, result.ErrorCode);
        Assert.Equal("Conta não encontrada.", result.ErrorMessage);

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Account>()), Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Deve retornar erro se ocorrer exceção inesperada")]
    public async Task DeleteAsync_ShouldFail_WhenExceptionThrown()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                       .ThrowsAsync(new Exception("Erro simulado de banco de dados."));

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
        Assert.Equal("Ocorreu um erro ao excluir a conta.", result.ErrorMessage);

        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Account>()), Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}
