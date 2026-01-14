using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
