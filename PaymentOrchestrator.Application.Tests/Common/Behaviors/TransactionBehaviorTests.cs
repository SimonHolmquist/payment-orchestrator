using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;
using PaymentOrchestrator.Application.Common.Behaviors;
using PaymentOrchestrator.Application.Tests.Fakes;
using Xunit;

namespace PaymentOrchestrator.Application.Tests.Common.Behaviors;

public class TransactionBehaviorTests
{
    private sealed record TxCommand() : ITransactionalRequest<int>;
    private sealed record NonTxQuery() : IRequest<int>;

    [Fact]
    public async Task Handle_WhenRequestIsTransactional_ExecutesInsideUnitOfWorkTransaction()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        var behavior = new TransactionBehavior<TxCommand, int>(uow);

        var request = new TxCommand();

        var nextCalls = 0;
        Task<int> Next()
        {
            nextCalls++;
            return Task.FromResult(123);
        }

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(1, nextCalls);
        Assert.Equal(1, uow.ExecuteInTransactionCalls);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotTransactional_DoesNotUseTransaction()
    {
        // Arrange
        var uow = new FakeUnitOfWork();
        var behavior = new TransactionBehavior<NonTxQuery, int>(uow);

        var request = new NonTxQuery();

        var nextCalls = 0;
        Task<int> Next()
        {
            nextCalls++;
            return Task.FromResult(456);
        }

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.Equal(456, result);
        Assert.Equal(1, nextCalls);
        Assert.Equal(0, uow.ExecuteInTransactionCalls);
    }
}
