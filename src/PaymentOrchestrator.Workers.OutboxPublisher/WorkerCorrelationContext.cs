using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Workers.OutboxPublisher;

public sealed class WorkerCorrelationContext : ICorrelationContext
{
    // Genera un ID único para cada ciclo de ejecución del worker
    public string CorrelationId { get; } = Guid.NewGuid().ToString();
}