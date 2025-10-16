using KRT.BankAccounts.Api._03_Domain.Entities;

namespace KRT.BankAccounts.Api._01_Presentation.DTOs.Response
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cpf { get; set; }
        public string Status { get; set; }

        public AccountResponse() { }

        public AccountResponse(Account account)
        {
            Id = account.Id;
            Name = account.Name;
            Cpf = account.Cpf;
            Status = account.Status.ToString();
        }
    }
}
