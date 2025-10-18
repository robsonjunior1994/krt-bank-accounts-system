using KRT.BankAccounts.Api._03_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._04_Infrastructure.Data
{
    [ExcludeFromCodeCoverage]
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
