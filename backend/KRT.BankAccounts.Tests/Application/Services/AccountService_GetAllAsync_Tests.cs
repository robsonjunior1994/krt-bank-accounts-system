using KRT.BankAccounts.Api._01_Presentation.Helpers;
using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._02_Application.Services;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using Microsoft.Extensions.Options;
using Moq;

namespace KRT.BankAccounts.Tests.Application.Services;

public class AccountService_GetAllAsync_Tests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<IOptions<CacheSettings>> _settingsMock;
    private readonly AccountService _service;

    public AccountService_GetAllAsync_Tests()
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

    [Fact(DisplayName = "Deve retornar contas com sucesso e aplicar paginação")]
    public async Task GetAllAsync_ShouldReturnPagedAccounts_WhenSuccess()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 2;

        var accounts = new List<Account>
        {
            new Account("Robson", "12345678901"),
            new Account("Junior", "98765432100"),
            new Account("Fernanda", "55555555555")
        };

        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(accounts.Count);
        _repositoryMock.Setup(r => r.GetPagedAsync(pageNumber, pageSize))
                       .ReturnsAsync(accounts.Take(pageSize));

        // Act
        var result = await _service.GetAllAsync(pageNumber, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Items.Count());
        Assert.Equal(pageNumber, result.Data.CurrentPage);
        Assert.Equal(pageSize, result.Data.PageSize);

        _repositoryMock.Verify(r => r.CountAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetPagedAsync(pageNumber, pageSize), Times.Once);
    }

    [Fact(DisplayName = "Deve retornar sucesso com lista vazia quando não houver contas")]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoAccountsFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(0);
        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
                       .ReturnsAsync(new List<Account>());

        // Act
        var result = await _service.GetAllAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
        Assert.Equal(0, result.Data.TotalCount);
        Assert.Equal(1, result.Data.CurrentPage);
        Assert.Equal(10, result.Data.PageSize);
        Assert.Equal(0, result.Data.TotalPages);

        _repositoryMock.Verify(r => r.GetPagedAsync(1, 10), Times.Once);
    }


    [Fact(DisplayName = "Deve falhar ao ocorrer exceção no repositório")]
    public async Task GetAllAsync_ShouldFail_WhenRepositoryThrowsException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CountAsync())
                       .ThrowsAsync(new Exception("Erro simulado de banco de dados"));

        // Act
        var result = await _service.GetAllAsync(1, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
        Assert.Equal("Ocorreu um erro ao buscar as contas.", result.ErrorMessage);
    }

    [Fact(DisplayName = "Deve retornar página parcial quando houver menos registros que o pageSize")]
    public async Task GetAllAsync_ShouldHandlePartialPage()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 5;
        var totalAccounts = 6;

        var accounts = new List<Account>
        {
            new Account("Robson", "12345678901")
        };

        _repositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(totalAccounts);
        _repositoryMock.Setup(r => r.GetPagedAsync(pageNumber, pageSize))
                       .ReturnsAsync(accounts);

        // Act
        var result = await _service.GetAllAsync(pageNumber, pageSize);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(totalAccounts, result.Data.TotalCount);
        Assert.Equal(1, result.Data.Items.Count());
        Assert.Equal(pageNumber, result.Data.CurrentPage);
    }
}
