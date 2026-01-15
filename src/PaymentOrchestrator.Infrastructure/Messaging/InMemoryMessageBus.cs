using Microsoft.Extensions.Logging;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Infrastructure.Messaging;

public sealed class InMemoryMessageBus(ILogger<InMemoryMessageBus> logger) : IMessageBus
{
    public Task PublishAsync(string type, string payload, CancellationToken cancellationToken = default)
    {
        // Simulación: En M10 aquí conectaremos RabbitMQ
        logger.LogInformation("📢 [BUS PUBLISH] Type: {Type} | Payload: {Payload}", type, payload);
        return Task.CompletedTask;
    }
}