namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<IdempotencyGetResult?> GetAsync(string clientId, string key, string operationName, CancellationToken ct);

    /// <summary>
    /// Intenta crear (ClientId, Key, OperationName) de forma atómica.
    /// Devuelve false si ya existe.
    /// </summary>
    Task<bool> TryCreateAsync(IdempotencyRecord record, CancellationToken ct);

    Task SaveResponseAsync(string clientId, string key, string operationName, IdempotencyResponseSnapshot response, DateTimeOffset completedAt, CancellationToken ct);
}