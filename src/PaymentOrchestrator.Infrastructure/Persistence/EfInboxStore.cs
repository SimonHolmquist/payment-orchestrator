using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfInboxStore(PaymentOrchestratorDbContext db) : IInboxStore
{
    public async Task<bool> TryBeginProcessAsync(
        string provider,
        string providerEventId,
        string eventType,
        string rawPayload,
        DateTimeOffset receivedAt,
        CancellationToken ct)
    {
        var entity = new InboxMessage
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            ProviderEventId = providerEventId,
            EventType = eventType,
            RawPayload = rawPayload,
            ReceivedAt = receivedAt
        };

        db.InboxMessages.Add(entity);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task MarkProcessedAsync(string provider, string providerEventId, DateTimeOffset processedAt, CancellationToken ct)
    {
        var msg = await db.InboxMessages
            .FirstAsync(x => x.Provider == provider && x.ProviderEventId == providerEventId, ct);

        msg.ProcessedAt = processedAt;
        msg.Error = null;

        await db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(string provider, string providerEventId, DateTimeOffset processedAt, string error, CancellationToken ct)
    {
        var msg = await db.InboxMessages
            .FirstAsync(x => x.Provider == provider && x.ProviderEventId == providerEventId, ct);

        msg.ProcessedAt = processedAt;
        msg.Error = error;

        await db.SaveChangesAsync(ct);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        // SQL Server unique constraint violation: 2601 / 2627
        if (ex.InnerException is SqlException sqlEx)
            return sqlEx.Number is 2601 or 2627;

        return false;
    }
}
