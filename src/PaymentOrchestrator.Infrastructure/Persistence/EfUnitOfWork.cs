using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public class EfUnitOfWork(PaymentOrchestratorDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        // Start a new transaction (or join ambient one if provider supports it)
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);

            // Ensure changes are flushed before committing.
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            // Best-effort rollback then rethrow
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch
            {
                // swallow secondary exceptions to preserve original error
            }

            throw;
        }
    }
}