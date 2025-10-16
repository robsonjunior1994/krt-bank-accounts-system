using KRT.BankAccounts.Api._02_Application.Interfaces.Services;
using KRT.BankAccounts.Api._02_Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KRT.BankAccounts.Api._02_Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();

            return services;
        }
    }
}
