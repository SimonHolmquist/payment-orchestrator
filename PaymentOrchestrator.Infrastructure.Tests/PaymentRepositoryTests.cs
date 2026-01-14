using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces; // Necesario para ICorrelationContext
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.Events;
using PaymentOrchestrator.Domain.Payments.ValueObjects;
using PaymentOrchestrator.Infrastructure.Persistence;
using System.Text.Json;
using Testcontainers.MsSql;
using Xunit;

namespace PaymentOrchestrator.Infrastructure.Tests;

public sealed class PaymentRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync() => await _mssql.StartAsync();

    public async Task DisposeAsync() => await _mssql.DisposeAsync();

    private PaymentOrchestratorDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PaymentOrchestratorDbContext>()
            .UseSqlServer(_mssql.GetConnectionString())
            .Options;

        var context = new PaymentOrchestratorDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    // Fake para el test (necesario para el UnitOfWork)
    private sealed class FakeCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation-id";
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPayment_AndOutboxMessages()
    {
        // Arrange
        using var context = CreateContext();

        // CORRECCIÓN: Instanciamos el UoW para que maneje la lógica de Outbox
        var uow = new EfUnitOfWork(context, new FakeCorrelationContext());
        var repository = new EfPaymentRepository(context);

        var payment = Payment.Create(
            PaymentId.New(),
            "client-test",
            Money.Of(150.00m, "USD"),
            DateTimeOffset.UtcNow);

        // Act
        await repository.AddAsync(payment, CancellationToken.None);

        // CORRECCIÓN: Usamos uow.SaveChangesAsync() en lugar de context.SaveChangesAsync()
        // Esto dispara EnqueueDomainEventsToOutbox() internamente.
        await uow.SaveChangesAsync();

        // Assert
        using var assertContext = CreateContext();

        // 1. Verificar Payment
        var persisted = await assertContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);
        Assert.NotNull(persisted);
        Assert.Equal(150.00m, persisted.Amount.Amount);
        Assert.Equal("USD", persisted.Amount.Currency);
        Assert.Equal(PaymentStatus.Created, persisted.Status);

        // 2. Verificar Outbox (Ahora sí debería existir)
        var outboxMsg = await assertContext.OutboxMessages.FirstOrDefaultAsync();

        Assert.NotNull(outboxMsg); // Aquí fallaba antes
        Assert.Equal(payment.Id.Value, outboxMsg.AggregateId);
        Assert.Equal("Payment", outboxMsg.AggregateType);
        Assert.Contains("PaymentCreated", outboxMsg.Type);
        Assert.Equal("test-correlation-id", outboxMsg.CorrelationId);

        // 3. Verificar Contenido JSON
        var json = JsonSerializer.Deserialize<JsonElement>(outboxMsg.Content);
        Assert.Equal(payment.Id.Value.ToString(), json.GetProperty("PaymentId").GetProperty("Value").GetString());
    }
}