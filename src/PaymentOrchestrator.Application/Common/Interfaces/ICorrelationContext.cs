namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface ICorrelationContext
{
    string CorrelationId { get; }
}