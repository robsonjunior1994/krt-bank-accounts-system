using KRT.BankAccounts.Api._03_Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._01_Presentation.DTOs.Response
{
    [ExcludeFromCodeCoverage]
    public class AccountResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cpf { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AccountResponse() { }

        public AccountResponse(Account account)
        {
            Id = account.Id;
            Name = account.Name;
            Cpf = account.Cpf;
            Status = account.Status.ToString();
            CreatedAt = account.CreatedAt;
            UpdatedAt = account.UpdatedAt;
        }
    }
}
