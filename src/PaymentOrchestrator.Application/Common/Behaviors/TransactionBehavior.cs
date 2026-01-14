using MediatR;
using PaymentOrchestrator.Application.Common.Abstractions;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Application.Common.Behaviors;

// Add constraint: where TRequest : IRequest<TResponse>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> // <-- Fix: add this constraint
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
