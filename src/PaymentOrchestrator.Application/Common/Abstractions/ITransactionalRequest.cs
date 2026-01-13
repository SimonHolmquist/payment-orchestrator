using MediatR;

namespace PaymentOrchestrator.Application.Common.Abstractions;

public interface ITransactionalRequest<out TResponse> : IRequest<TResponse>;