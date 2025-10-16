using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using KRT.BankAccounts.Api._04_Infrastructure.Messaging;
using KRT.BankAccounts.Api._04_Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KRT.BankAccounts.Api._04_Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

            return services;
        }
    }
}
