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
    public class AccountService_Create_Tests
    {
        private readonly Mock<IAccountRepository> _repositoryMock;
        private readonly Mock<IMessagePublisher> _publisherMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IOptions<CacheSettings>> _settingsMock;
        private readonly AccountService _service;

        public AccountService_Create_Tests()
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

        [Fact(DisplayName = "Deve criar conta com sucesso")]
        public async Task CreateAsync_ShouldCreateAccountSuccessfully()
        {
            // Arrange
            var request = new CreateAccountRequest { Name = "Robson Junior", Cpf = "12345678901" };

            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf))
                           .ReturnsAsync(false);

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Account>()))
                           .Returns(Task.CompletedTask);

            _publisherMock.Setup(p => p.PublishAsync("account.created", It.IsAny<object>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Robson Junior", result.Data.Name);
            Assert.Equal("12345678901", result.Data.Cpf);
            Assert.Null(result.ErrorCode);
        }

        [Fact(DisplayName = "Não deve criar conta com CPF duplicado")]
        public async Task CreateAsync_ShouldFail_WhenCpfAlreadyExists()
        {
            // Arrange
            var request = new CreateAccountRequest { Name = "Robson", Cpf = "99988877766" };
            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf))
                           .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.RESOURCE_ALREADY_EXISTS, result.ErrorCode);
            Assert.Equal("CPF já cadastrado.", result.ErrorMessage);
        }

        [Fact(DisplayName = "Não deve criar conta com nome inválido")]
        public async Task CreateAsync_ShouldFail_WhenNameIsTooShort()
        {
            // Arrange
            var request = new CreateAccountRequest { Name = "Jo", Cpf = "11122233344" };

            _repositoryMock.Setup(r => r.ExistsByCpfAsync(request.Cpf))
                           .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.ErrorCode);
            Assert.Equal("O nome deve conter ao menos 3 caracteres.", result.ErrorMessage);
        }

        [Fact(DisplayName = "Deve falhar ao ocorrer exceção interna")]
        public async Task CreateAsync_ShouldFail_WhenExceptionThrown()
        {
            // Arrange
            var request = new CreateAccountRequest { Name = "Carlos", Cpf = "44455566677" };

            _repositoryMock.Setup(r => r.ExistsByCpfAsync(It.IsAny<string>()))
                           .ThrowsAsync(new Exception("Erro de banco de dados simulado."));

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.ErrorCode);
            Assert.Equal("Ocorreu um erro ao criar a conta.", result.ErrorMessage);
        }
    }
}
