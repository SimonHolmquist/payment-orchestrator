using FluentValidation;

namespace PaymentOrchestrator.Application.Payments.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(v => v.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");

        RuleFor(v => v.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(v => v.Currency)
            .NotEmpty()
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}