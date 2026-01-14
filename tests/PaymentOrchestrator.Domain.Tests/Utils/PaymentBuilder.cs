using PaymentOrchestrator.Domain.Payments;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Tests.Utils;

public class PaymentBuilder
{
    private PaymentId _id = PaymentId.New();
    private string _clientId = "test-client";
    private Money _amount = Money.Of(100, "USD");
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    // Configuración de estado objetivo
    private bool _authorize = false;
    private decimal? _captureAmount = null;

    public PaymentBuilder WithId(Guid id)
    {
        _id = new PaymentId(id);
        return this;
    }

    public PaymentBuilder WithAmount(decimal amount, string currency = "USD")
    {
        _amount = Money.Of(amount, currency);
        return this;
    }

    public PaymentBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    public PaymentBuilder Authorized()
    {
        _authorize = true;
        return this;
    }

    public PaymentBuilder Captured(decimal amount)
    {
        _authorize = true; // No se puede capturar sin autorizar
        _captureAmount = amount;
        return this;
    }

    public Payment Build()
    {
        // 1. Crear (Estado inicial)
        var payment = Payment.Create(_id, _clientId, _amount, _now);

        // 2. Transición a Autorizado si se requiere
        if (_authorize)
        {
            payment.MarkAuthorized("psp-ref-default", _now.AddMinutes(1));
        }

        // 3. Transición a Capturado si se requiere
        if (_captureAmount.HasValue)
        {
            payment.MarkCapture(_captureAmount.Value, _now.AddMinutes(2));
        }

        // Opcional: Limpiar eventos si solo nos importa el estado final para el setup
        // payment.DequeueDomainEvents();

        return payment;
    }
}