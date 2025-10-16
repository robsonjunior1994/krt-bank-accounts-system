using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KRT.BankAccounts.Api._04_Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account> GetByIdAsync(int id) =>
            await _context.Accounts.FindAsync(id);

        public async Task<IEnumerable<Account>> GetAllAsync() =>
            await _context.Accounts.ToListAsync();

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Account account)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByCpfAsync(string cpf) =>
            await _context.Accounts.AnyAsync(x => x.Cpf == cpf);
    }
}
