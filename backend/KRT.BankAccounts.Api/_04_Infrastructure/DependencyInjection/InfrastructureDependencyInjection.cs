using KRT.BankAccounts.Api._02_Application.Interfaces.Infra;
using KRT.BankAccounts.Api._03_Domain.Interfaces;
using KRT.BankAccounts.Api._04_Infrastructure.Cache;
using KRT.BankAccounts.Api._04_Infrastructure.Messaging;
using KRT.BankAccounts.Api._04_Infrastructure.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._04_Infrastructure.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddSingleton<ICacheService, RedisCacheService>();

            // Forma de publicar mais simples, não existe a implementação de publicação async
            // Estou deixando esse comentário aqui para comparação e debate
            //services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
            services.AddSingleton<IMessagePublisher, EasyNetQMessagePublisher>();

            return services;
        }
    }
}
