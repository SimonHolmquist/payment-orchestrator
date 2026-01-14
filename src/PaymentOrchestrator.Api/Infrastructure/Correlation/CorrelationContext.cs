using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Api.Infrastructure.Correlation;

public sealed class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public string CorrelationId => _current.Value ?? "missing-correlation-id";

    public static void Set(string correlationId) => _current.Value = correlationId;
}
