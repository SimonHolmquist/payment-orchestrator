using FluentValidation;
using PaymentOrchestrator.Application.Common.Behaviors;
using PaymentOrchestrator.Application.Payments.Commands.CreatePayment;
using Xunit;

namespace PaymentOrchestrator.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ThrowsValidationException()
    {
        // Arrange
        var validators = new IValidator<CreatePaymentCommand>[]
        {
            new CreatePaymentCommandValidator()
        };

        var behavior = new ValidationBehavior<CreatePaymentCommand, Guid>(validators);

        var invalid = new CreatePaymentCommand(
            ClientId: "",      // invalid
            Amount: 0m,        // invalid
            Currency: "AR"     // invalid (length != 3)
        );

        static Task<Guid> Next() => Task.FromResult(Guid.NewGuid());

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(invalid, Next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CallsNextAndReturnsResponse()
    {
        // Arrange
        var validators = new IValidator<CreatePaymentCommand>[]
        {
            new CreatePaymentCommandValidator()
        };

        var behavior = new ValidationBehavior<CreatePaymentCommand, Guid>(validators);

        var valid = new CreatePaymentCommand(
            ClientId: "client-123",
            Amount: 100.50m,
            Currency: "ARS"
        );

        var expected = Guid.NewGuid();
        Task<Guid> Next() => Task.FromResult(expected);

        // Act
        var result = await behavior.Handle(valid, Next, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
    }
}
