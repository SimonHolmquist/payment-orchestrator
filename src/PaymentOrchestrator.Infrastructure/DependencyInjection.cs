using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Persistence.Repositories;

namespace PaymentOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("SqlServer")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer");

        services.AddDbContext<PaymentOrchestratorDbContext>(opt => opt.UseSqlServer(cs));

        services.AddScoped<IPaymentRepository, Persistence.EfPaymentRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IInboxStore, EfInboxStore>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

        return services;
    }
}
