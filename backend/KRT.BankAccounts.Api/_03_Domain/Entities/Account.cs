using KRT.BankAccounts.Api._03_Domain.Enums;

namespace KRT.BankAccounts.Api._03_Domain.Entities
{
    public class Account
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Cpf { get; private set; }
        public AccountStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; } 

        protected Account() { }

        public Account(string nomeTitular, string cpf)
        {
            Name = nomeTitular;
            Cpf = cpf;
            Status = AccountStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (Status == AccountStatus.Active)
                throw new InvalidOperationException("A conta já está ativa.");

            Status = AccountStatus.Active;
            Update();
        }

        public void Deactivate()
        {
            if (Status == AccountStatus.Inactive)
                throw new InvalidOperationException("A conta já está inativa.");

            Status = AccountStatus.Inactive;
            Update();
        }

        public void Update()
        {
             UpdatedAt = DateTime.UtcNow;
        }
    }
}
