using MediatR;

namespace PaymentOrchestrator.Application.Webhooks.Commands.ProcessPspWebhook;

public sealed record ProcessPspWebhookCommand(
    string Provider,
    string ProviderEventId,
    string EventType,
    Guid PaymentId,
    string RawPayload)
    : IRequest;
