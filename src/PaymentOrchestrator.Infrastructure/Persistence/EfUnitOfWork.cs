using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public class EfUnitOfWork(PaymentOrchestratorDbContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}