using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;
using PaymentOrchestrator.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace PaymentOrchestrator.Infrastructure.Tests.Persistence;

public sealed class ConcurrencyTests : IAsyncLifetime
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

        var ctx = new PaymentOrchestratorDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Update_WhenRowVersionMismatch_ShouldThrowConcurrencyException()
    {
        // 1. Arrange: Crear un pago inicial
        var paymentId = PaymentId.New();
        using (var ctx = CreateContext())
        {
            var payment = Payment.Create(paymentId, "client-1", Money.Of(100, "USD"), DateTimeOffset.UtcNow);
            ctx.Payments.Add(payment);
            await ctx.SaveChangesAsync();
        }

        // 2. Act: Simular dos usuarios leyendo el MISMO registro al mismo tiempo
        using var ctxUser1 = CreateContext();
        using var ctxUser2 = CreateContext();

        var paymentUser1 = await ctxUser1.Payments.FindAsync(paymentId);
        var paymentUser2 = await ctxUser2.Payments.FindAsync(paymentId);

        Assert.NotNull(paymentUser1);
        Assert.NotNull(paymentUser2);

        // 3. User 1 modifica y guarda (esto actualiza el RowVersion en BD)
        paymentUser1.MarkAuthorized("auth-ref-1", DateTimeOffset.UtcNow);
        await ctxUser1.SaveChangesAsync();

        // 4. User 2 intenta modificar SU versión (que ahora es vieja/stale)
        paymentUser2.MarkAuthorized("auth-ref-2", DateTimeOffset.UtcNow);

        // 5. Assert: Debe explotar porque RowVersion de User2 != RowVersion en BD
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await ctxUser2.SaveChangesAsync());
    }
}