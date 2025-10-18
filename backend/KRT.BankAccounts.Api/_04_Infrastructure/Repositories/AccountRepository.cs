using KRT.BankAccounts.Api._03_Domain.Entities;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._04_Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
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
        public async Task<IEnumerable<Account>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Accounts
                .OrderBy(a => a.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Accounts.CountAsync();
        }

    }
}
