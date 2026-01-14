using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;
using PaymentOrchestrator.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace PaymentOrchestrator.Infrastructure.Tests;

public sealed class PaymentRepositoryTests : IAsyncLifetime
{
    // CORRECCIÓN: Usamos el constructor con la imagen explícita
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

    [Fact]
    public async Task AddAsync_ShouldPersistPayment_AndOutboxMessages()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EfPaymentRepository(context);

        var payment = Payment.Create(
            PaymentId.New(),
            "client-test",
            Money.Of(150.00m, "USD"),
            DateTimeOffset.UtcNow);

        // Act
        await repository.AddAsync(payment, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        using var assertContext = CreateContext();
        var persisted = await assertContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);

        Assert.NotNull(persisted);
        Assert.Equal(150.00m, persisted.Amount.Amount);
        Assert.Equal("USD", persisted.Amount.Currency);
        Assert.Equal(PaymentStatus.Created, persisted.Status);
    }
}