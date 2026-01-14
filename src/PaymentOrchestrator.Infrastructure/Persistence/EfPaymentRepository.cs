using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Domain.Payments;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfPaymentRepository(PaymentOrchestratorDbContext db) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await db.Payments.AddAsync(payment, ct);

    public Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken ct = default)
        => db.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        return Task.CompletedTask;
    }
}
