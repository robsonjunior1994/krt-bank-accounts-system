using KRT.BankAccounts.Api._03_Domain.Entities;

namespace KRT.BankAccounts.Api._03_Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account> GetByIdAsync(int id);
        Task<IEnumerable<Account>> GetAllAsync();
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(Account account);
        Task<bool> ExistsByCpfAsync(string cpf);
    }
}
