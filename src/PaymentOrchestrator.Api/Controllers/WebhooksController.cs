using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Application.Webhooks.Commands.ProcessPspWebhook;
using System.Text.Json;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("webhooks")]
public sealed class WebhooksController(IMediator mediator) : ControllerBase
{
    [HttpPost("{psp}")]
    public async Task<IActionResult> Ingest(string psp, [FromBody] JsonElement body, CancellationToken ct)
    {
        // M3 mínimo: asumimos que el webhook trae estos campos.
        // En M6 reemplazás por contratos por provider + signature validation.
        var providerEventId = body.GetProperty("eventId").GetString() ?? Guid.NewGuid().ToString("N");
        var eventType = body.GetProperty("type").GetString() ?? "unknown";
        var paymentId = Guid.Parse(body.GetProperty("paymentId").GetString()!);

        await mediator.Send(new ProcessPspWebhookCommand(
            Provider: psp,
            ProviderEventId: providerEventId,
            EventType: eventType,
            PaymentId: paymentId,
            RawPayload: body.GetRawText()
        ), ct);

        return Ok();
    }
}
