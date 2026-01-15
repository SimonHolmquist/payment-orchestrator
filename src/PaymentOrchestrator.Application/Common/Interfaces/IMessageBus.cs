namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IMessageBus
{
    Task PublishAsync(string type, string payload, CancellationToken cancellationToken = default);
}