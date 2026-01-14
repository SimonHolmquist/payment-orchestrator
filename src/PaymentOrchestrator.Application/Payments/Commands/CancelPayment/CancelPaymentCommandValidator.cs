using FluentValidation;

namespace PaymentOrchestrator.Application.Payments.Commands.CancelPayment;

public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
