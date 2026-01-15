using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure.Persistence;

namespace PaymentOrchestrator.Workers.OutboxPublisher;

public sealed class Worker(
    IServiceProvider serviceProvider,
    ILogger<Worker> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🚀 Outbox Publisher Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error processing outbox batch.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentOrchestratorDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        // 1. Obtener mensajes pendientes (Locking optimista vía concurrencia normal)
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        logger.LogInformation("Processing {Count} outbox messages...", messages.Count);

        foreach (var msg in messages)
        {
            try
            {
                // 2. Publicar
                await bus.PublishAsync(msg.Type, msg.Content, ct);

                // 3. Marcar como procesado
                msg.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish message {MessageId}", msg.Id);
                // En un escenario real, aquí podríamos incrementar un contador de reintentos
                // o mover a una Dead Letter Queue en la DB.
            }
        }

        // 4. Guardar cambios (actualiza ProcessedAt)
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Batch processed successfully.");
    }
}