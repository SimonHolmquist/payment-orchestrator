using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Persistence.Interceptors;
using PaymentOrchestrator.Infrastructure.Psp; // Importante

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("SqlServer")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer");

        // Registrar el Interceptor
        services.AddScoped<DomainEventsToOutboxInterceptor>();

        services.AddDbContext<PaymentOrchestratorDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(cs);
            // Inyectar el interceptor resuelto desde el contenedor
            opt.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
        });

        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        // EfUnitOfWork ya no necesita ICorrelationContext en su constructor
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IInboxStore, EfInboxStore>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        services.AddSingleton<IPspClient, NullPspClient>();


        return services;
    }
}