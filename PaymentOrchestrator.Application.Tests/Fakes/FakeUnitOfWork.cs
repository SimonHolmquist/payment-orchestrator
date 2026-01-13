using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }
    public int ExecuteInTransactionCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ExecuteInTransactionCalls++;
        return await action(cancellationToken);
    }
}
