using FluentValidation;

namespace PaymentOrchestrator.Application.Payments.Commands.AuthorizePayment;

public class AuthorizePaymentCommandValidator : AbstractValidator<AuthorizePaymentCommand>
{
    public AuthorizePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.PspReference).NotEmpty();
    }
}
