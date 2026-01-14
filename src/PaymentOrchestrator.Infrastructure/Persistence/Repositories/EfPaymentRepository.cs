using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Infrastructure.Persistence.Repositories;

public class EfPaymentRepository(PaymentOrchestratorDbContext context) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await context.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
