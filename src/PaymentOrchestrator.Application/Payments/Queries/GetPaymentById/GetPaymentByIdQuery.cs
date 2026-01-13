using MediatR;

namespace PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto?>;