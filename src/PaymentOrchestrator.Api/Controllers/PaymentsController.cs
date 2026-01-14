using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Api.Contracts.Payments;
using PaymentOrchestrator.Api.Infrastructure.Idempotency; // Asegúrate de tener este using
using PaymentOrchestrator.Application.Payments.Commands.AuthorizePayment;
using PaymentOrchestrator.Application.Payments.Commands.CancelPayment;
using PaymentOrchestrator.Application.Payments.Commands.CapturePayment;
using PaymentOrchestrator.Application.Payments.Commands.CreatePayment;
using PaymentOrchestrator.Application.Payments.Commands.RefundPayment;
using PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("payments")]
// CORRECCIÓN: Eliminamos IIdempotencyStore y IClock del constructor primario
public sealed class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Idempotent(Operation = "CreatePayment")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req, CancellationToken ct)
    {
        // El filtro maneja la idempotencia. Aquí solo orquestamos.
        var paymentId = await mediator.Send(new CreatePaymentCommand(req.ClientId, req.Amount, req.Currency), ct);

        // Retornamos CreatedAtAction para que el filtro pueda serializar la respuesta 201
        return CreatedAtAction(nameof(GetById), new { id = paymentId }, new { paymentId });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetPaymentByIdQuery(id), ct);
        return Ok(dto);
    }

    [HttpPost("{id:guid}/authorize")]
    [Idempotent(Operation = "AuthorizePayment")]
    public async Task<IActionResult> Authorize(Guid id, [FromBody] AuthorizeRequest req, CancellationToken ct)
    {
        await mediator.Send(new AuthorizePaymentCommand(id, req.PspReference), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/capture")]
    [Idempotent(Operation = "CapturePayment")]
    public async Task<IActionResult> Capture(Guid id, [FromBody] CaptureRequest req, CancellationToken ct)
    {
        await mediator.Send(new CapturePaymentCommand(id, req.Amount), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/refund")]
    [Idempotent(Operation = "RefundPayment")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest req, CancellationToken ct)
    {
        await mediator.Send(new RefundPaymentCommand(id, req.Amount), ct);
        return Accepted();
    }

    [HttpPost("{id:guid}/cancel")]
    [Idempotent(Operation = "CancelPayment")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await mediator.Send(new CancelPaymentCommand(id), ct);
        return Accepted();
    }
}