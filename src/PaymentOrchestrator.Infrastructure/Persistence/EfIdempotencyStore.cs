using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence.Entities;

namespace PaymentOrchestrator.Infrastructure.Persistence;

public sealed class EfIdempotencyStore(PaymentOrchestratorDbContext db) : IIdempotencyStore
{
    public async Task<IdempotencyGetResult?> GetAsync(string clientId, string key, string operationName, CancellationToken ct)
    {
        var row = await db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == clientId && x.Key == key && x.OperationName == operationName, ct);

        if (row is null) return null;

        IdempotencyResponseSnapshot? response = null;
        if (row.ResponseStatusCode.HasValue && row.ResponseBody is not null && row.ResponseContentType is not null)
        {
            response = new IdempotencyResponseSnapshot(row.ResponseStatusCode.Value, row.ResponseContentType, row.ResponseBody);
        }

        return new IdempotencyGetResult(row.ClientId, row.Key, row.OperationName, row.RequestHash, response);
    }

    public async Task<bool> TryCreateAsync(IdempotencyRecord record, CancellationToken ct)
    {
        db.IdempotencyKeys.Add(new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            ClientId = record.ClientId,
            Key = record.Key,
            OperationName = record.OperationName,
            RequestHash = record.RequestHash,
            CreatedAt = record.CreatedAt
        });

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

    public async Task SaveResponseAsync(string clientId, string key, string operationName, IdempotencyResponseSnapshot response, DateTimeOffset completedAt, CancellationToken ct)
    {
        var row = await db.IdempotencyKeys
            .FirstAsync(x => x.ClientId == clientId && x.Key == key && x.OperationName == operationName, ct);

        row.ResponseStatusCode = response.StatusCode;
        row.ResponseContentType = response.ContentType;
        row.ResponseBody = response.Body;
        row.CompletedAt = completedAt;

        await db.SaveChangesAsync(ct);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
            return sqlEx.Number is 2601 or 2627;

        return false;
    }
}
