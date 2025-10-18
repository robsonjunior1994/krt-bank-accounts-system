using System;
using System.Reflection;
using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Enums;
using Xunit;

namespace KRT.BankAccounts.Tests.Domain.Entities
{
    public class Account_Tests
    {
        [Fact(DisplayName = "Deve instanciar conta com nome e CPF válidos")]
        public void Constructor_ShouldCreateAccountWithValidData()
        {
            // Act
            var account = new Account("Robson", "12345678901");

            // Assert
            Assert.Equal("Robson", account.Name);
            Assert.Equal("12345678901", account.Cpf);
            Assert.Equal(AccountStatus.Active, account.Status);
            Assert.True(account.CreatedAt <= DateTime.UtcNow);
            Assert.Equal(default, account.UpdatedAt); // ainda não atualizado
        }

        [Fact(DisplayName = "Deve permitir instanciar via construtor protegido (para EF Core)")]
        public void ProtectedConstructor_ShouldInstantiateForEFCore()
        {
            // Arrange
            var constructor = typeof(Account).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);

            // Act
            var account = (Account)constructor.Invoke(null);

            // Assert
            Assert.NotNull(account);
            Assert.Null(account.Name);
            Assert.Null(account.Cpf);
            Assert.Equal(default, account.Status);
            Assert.Equal(default, account.CreatedAt);
            Assert.Equal(default, account.UpdatedAt);
        }

        [Fact(DisplayName = "Deve atualizar nome e CPF com sucesso")]
        public void Update_ShouldChangeNameAndCpf_WhenValid()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            var oldUpdatedAt = account.UpdatedAt;

            // Act
            account.Update("Novo Nome", "10987654321");

            // Assert
            Assert.Equal("Novo Nome", account.Name);
            Assert.Equal("10987654321", account.Cpf);
            Assert.True(account.UpdatedAt > oldUpdatedAt);
        }

        [Fact(DisplayName = "Deve lançar exceção quando nome for vazio")]
        public void Update_ShouldThrow_WhenNameIsEmpty()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => account.Update("", "12345678901"));
            Assert.Equal("O nome não pode ser vazio.", ex.Message);
        }

        [Fact(DisplayName = "Deve lançar exceção quando CPF for inválido")]
        public void Update_ShouldThrow_WhenCpfInvalid()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");

            // Act & Assert
            var ex1 = Assert.Throws<InvalidOperationException>(() => account.Update("Robson", ""));
            Assert.Equal("O CPF deve conter 11 dígitos.", ex1.Message);

            var ex2 = Assert.Throws<InvalidOperationException>(() => account.Update("Robson", "123"));
            Assert.Equal("O CPF deve conter 11 dígitos.", ex2.Message);
        }

        [Fact(DisplayName = "Deve atualizar apenas UpdatedAt ao chamar Update() sem parâmetros")]
        public void Update_ShouldOnlyRefreshUpdatedAt_WhenCalledWithoutParameters()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            var oldUpdatedAt = account.UpdatedAt;

            // Act
            account.Update();

            // Assert
            Assert.True(account.UpdatedAt > oldUpdatedAt);
        }

        [Fact(DisplayName = "Deve lançar exceção ao ativar conta já ativa")]
        public void Activate_ShouldThrow_WhenAlreadyActive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => account.Activate());
            Assert.Equal("A conta já está ativa.", ex.Message);
        }

        [Fact(DisplayName = "Deve ativar conta inativa com sucesso")]
        public void Activate_ShouldWork_WhenInactive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            account.Deactivate();
            var oldUpdatedAt = account.UpdatedAt;

            // Act
            account.Activate();

            // Assert
            Assert.Equal(AccountStatus.Active, account.Status);
            Assert.True(account.UpdatedAt > oldUpdatedAt);
        }

        [Fact(DisplayName = "Deve lançar exceção ao desativar conta já inativa")]
        public void Deactivate_ShouldThrow_WhenAlreadyInactive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            account.Deactivate();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => account.Deactivate());
            Assert.Equal("A conta já está inativa.", ex.Message);
        }

        [Fact(DisplayName = "Deve desativar conta ativa com sucesso")]
        public void Deactivate_ShouldWork_WhenActive()
        {
            // Arrange
            var account = new Account("Robson", "12345678901");
            var oldUpdatedAt = account.UpdatedAt;

            // Act
            account.Deactivate();

            // Assert
            Assert.Equal(AccountStatus.Inactive, account.Status);
            Assert.True(account.UpdatedAt > oldUpdatedAt);
        }
    }
}
