using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentOrchestrator.Application.Payments.Commands.CreatePayment;
using PaymentOrchestrator.Application.Payments.Queries.GetPaymentById;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Crea un nuevo intento de pago.
    /// </summary>
    /// <param name="request">Datos del pago (Monto, Moneda, Cliente).</param>
    /// <param name="cancellationToken"></param>
    /// <returns>El ID del pago creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        // En M4 aquí leeremos el header Idempotency-Key
        var paymentId = await sender.Send(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = paymentId }, new { id = paymentId });
    }

    /// <summary>
    /// Obtiene los detalles de un pago por su ID.
    /// </summary>
    /// <param name="id">Identificador único del pago.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Detalles del pago o 404.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPaymentByIdQuery(id);
        var payment = await sender.Send(query, cancellationToken);

        return payment is not null ? Ok(payment) : NotFound();
    }

    // --- Endpoints futuros (scaffolding para completar M3) ---
    // Estos requieren crear los Commands correspondientes en Application (M1 extendido)

    /*
    [HttpPost("{id:guid}/authorize")]
    public async Task<IActionResult> Authorize(Guid id, [FromBody] AuthorizePaymentRequest req)
    {
        // await sender.Send(new AuthorizePaymentCommand(id, ...));
        return Ok();
    }

    [HttpPost("{id:guid}/capture")]
    public async Task<IActionResult> Capture(Guid id, [FromBody] CapturePaymentRequest req)
    {
        // await sender.Send(new CapturePaymentCommand(id, ...));
        return Ok();
    }
    */
}