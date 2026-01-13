using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Solo comandos (writes) — queries no.
        if (request is not ITransactionalRequest<TResponse>)
            return next();

        return uow.ExecuteInTransactionAsync(_ => next(), cancellationToken);
    }
}
