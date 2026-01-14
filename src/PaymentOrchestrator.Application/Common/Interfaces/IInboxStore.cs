namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IInboxStore
{
    /// <summary>
    /// Inserta un evento de inbox con dedupe por (Provider, ProviderEventId).
    /// Si ya existía, devuelve false (no procesar).
    /// </summary>
    Task<bool> TryBeginProcessAsync(
        string provider,
        string providerEventId,
        string eventType,
        string rawPayload,
        DateTimeOffset receivedAt,
        CancellationToken ct);

    Task MarkProcessedAsync(string provider, string providerEventId, DateTimeOffset processedAt, CancellationToken ct);

    Task MarkFailedAsync(string provider, string providerEventId, DateTimeOffset processedAt, string error, CancellationToken ct);
}