using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfUnitOfWork(PaymentOrchestratorDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // La lógica de Outbox/Ledger ahora vive en el Interceptor
        await db.SaveChangesAsync(ct);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await action(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
}