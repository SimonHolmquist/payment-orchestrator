using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Api.Contracts.Payments;
using PaymentOrchestrator.Application.Common.Exceptions;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Application.Payments.Commands.AuthorizePayment;
using PaymentOrchestrator.Application.Payments.Commands.CancelPayment;
using PaymentOrchestrator.Application.Payments.Commands.CapturePayment;
using PaymentOrchestrator.Application.Payments.Commands.CreatePayment;
using PaymentOrchestrator.Application.Payments.Commands.RefundPayment;
using PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController(IMediator mediator, IIdempotencyStore idempotency, IClock clock) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req, CancellationToken ct)
    {
        var operation = "CreatePayment";
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault()?.Trim();
        var requestHash = Sha256Hex(JsonSerializer.Serialize(req, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            // 1) si existe, devolvemos snapshot o conflict
            var existing = await idempotency.GetAsync(req.ClientId, idempotencyKey, operation, ct);
            if (existing is not null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                    throw new ConflictException("Idempotency-Key reuse with different request body.");

                if (existing.Response is not null)
                    return Content(existing.Response.Body, existing.Response.ContentType, Encoding.UTF8)
                        .WithStatusCode(existing.Response.StatusCode);

                // existe pero sin response: caemos a ejecutar (o podrías 409/425; acá ejecutamos)
            }
            else
            {
                var created = await idempotency.TryCreateAsync(
                    new IdempotencyRecord(req.ClientId, idempotencyKey, operation, requestHash, clock.UtcNow), ct);

                if (!created)
                {
                    // carrera: releer
                    existing = await idempotency.GetAsync(req.ClientId, idempotencyKey, operation, ct);
                    if (existing is not null && existing.Response is not null)
                        return Content(existing.Response.Body, existing.Response.ContentType, Encoding.UTF8)
                            .WithStatusCode(existing.Response.StatusCode);
                }
            }
        }

        var paymentId = await mediator.Send(new CreatePaymentCommand(req.ClientId, req.Amount, req.Currency), ct);

        var bodyJson = JsonSerializer.Serialize(new { paymentId });
        var responseSnapshot = new IdempotencyResponseSnapshot(201, "application/json", bodyJson);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await idempotency.SaveResponseAsync(req.ClientId, idempotencyKey!, operation, responseSnapshot, clock.UtcNow, ct);
        }

        return CreatedAtAction(nameof(GetById), new { id = paymentId }, new { paymentId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetPaymentByIdQuery(id), ct);
        return Ok(dto);
    }

    [HttpPost("{id:guid}/authorize")]
    public async Task<IActionResult> Authorize(Guid id, [FromBody] AuthorizeRequest req, CancellationToken ct)
    {
        await mediator.Send(new AuthorizePaymentCommand(id, req.PspReference), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/capture")]
    public async Task<IActionResult> Capture(Guid id, [FromBody] CaptureRequest req, CancellationToken ct)
    {
        await mediator.Send(new CapturePaymentCommand(id, req.Amount), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest req, CancellationToken ct)
    {
        await mediator.Send(new RefundPaymentCommand(id, req.Amount), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelPaymentCommand(id), ct);
        return Accepted();
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

file static class ActionResultExtensions
{
    public static ContentResult WithStatusCode(this ContentResult r, int statusCode)
    {
        r.StatusCode = statusCode;
        return r;
    }
}
