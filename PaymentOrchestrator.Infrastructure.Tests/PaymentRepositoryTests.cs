using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Necesario para simular DI
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;
using PaymentOrchestrator.Infrastructure.Persistence;
using PaymentOrchestrator.Infrastructure.Persistence.Interceptors;
using System.Text.Json;
using Testcontainers.MsSql;
using Xunit;

namespace PaymentOrchestrator.Infrastructure.Tests;

public sealed class PaymentRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync() => await _mssql.StartAsync();
    public async Task DisposeAsync() => await _mssql.DisposeAsync();

    // Fake para CorrelationContext
    private sealed class FakeCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation-xyz";
    }

    private IServiceProvider CreateServiceProvider
    {
        get
        {
            var services = new ServiceCollection();

            // Registrar DbContext con el Interceptor real
            services.AddScoped<ICorrelationContext, FakeCorrelationContext>();
            services.AddScoped<DomainEventsToOutboxInterceptor>();

            services.AddDbContext<PaymentOrchestratorDbContext>((sp, options) =>
            {
                options.UseSqlServer(_mssql.GetConnectionString());
                options.AddInterceptors(sp.GetRequiredService<DomainEventsToOutboxInterceptor>());
            });

            services.AddScoped<IPaymentRepository, EfPaymentRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            return services.BuildServiceProvider();
        }
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPayment_Outbox_And_Ledger()
    {
        // 1. Arrange: Crear scope y servicios
        var provider = CreateServiceProvider;
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentOrchestratorDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Asegurar BD creada
        await context.Database.EnsureCreatedAsync();

        var payment = Payment.Create(
            PaymentId.New(),
            "client-enterprise",
            Money.Of(99.99m, "EUR"),
            DateTimeOffset.UtcNow);

        // 2. Act: Añadir y Guardar
        await repo.AddAsync(payment, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None); // Esto dispara el Interceptor

        // 3. Assert: Verificar en un contexto limpio
        using var assertScope = provider.CreateScope();
        var assertContext = assertScope.ServiceProvider.GetRequiredService<PaymentOrchestratorDbContext>();

        // A. Verificar Payment
        var persistedPayment = await assertContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);
        Assert.NotNull(persistedPayment);
        Assert.Equal("EUR", persistedPayment.Amount.Currency);

        // B. Verificar Outbox (Requisito M2)
        var outboxMsg = await assertContext.OutboxMessages.FirstOrDefaultAsync();
        Assert.NotNull(outboxMsg);
        Assert.Equal("PaymentCreated", outboxMsg.Type);
        Assert.Equal("test-correlation-xyz", outboxMsg.CorrelationId);

        // C. Verificar Ledger (Requisito A - Enterprise)
        var ledgerEntry = await assertContext.PaymentLedger.FirstOrDefaultAsync();
        Assert.NotNull(ledgerEntry);
        Assert.Equal(payment.Id.Value, ledgerEntry.PaymentId);
        Assert.Equal("PaymentCreated", ledgerEntry.Action);
        Assert.Equal("test-correlation-xyz", ledgerEntry.CorrelationId);

        // Verificar snapshot JSON
        using var doc = JsonDocument.Parse(ledgerEntry.StateSnapshot!);
        Assert.Equal("Created", doc.RootElement.GetProperty("Status").GetString()); // Status enum como int -> string en JSON depende del serializer, pero aquí verificamos existencia
        Assert.Equal("client-enterprise", persistedPayment.ClientId);
    }
}