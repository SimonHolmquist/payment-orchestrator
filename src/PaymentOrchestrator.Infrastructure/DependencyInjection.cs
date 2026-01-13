using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Persistence.Interceptors;
using PaymentOrchestrator.Infrastructure.Persistence.Repositories;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        services.AddSingleton<DispatchDomainEventsInterceptor>();

        services.AddDbContext<PaymentOrchestratorDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}