using FluentValidation;

namespace PaymentOrchestrator.Application.Payments.Commands.ReconcilePayment;

public sealed class ReconcilePaymentCommandValidator : AbstractValidator<ReconcilePaymentCommand>
{
    public ReconcilePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("PaymentId is required.");
    }
}