using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = Guard.NonNegative(amount, nameof(amount));
        Currency = NormalizeCurrency(currency);
    }

    public static Money Of(decimal amount, string currency) => new(amount, currency);

    private static string NormalizeCurrency(string? currency)
    {
        currency = Guard.NotNullOrWhiteSpace(currency, nameof(currency)).ToUpperInvariant();

        // ISO-4217 style: 3 letters. (No validación “lista oficial” para no depender de datasets externos)
        if (currency.Length != 3 || currency.Any(c => c < 'A' || c > 'Z'))
            throw new DomainException("Currency must be a 3-letter ISO code (e.g., USD, EUR, ARS).");

        return currency;
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0) throw new DomainException("Money operation would result in a negative amount.");
        return new Money(result, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Currency mismatch.");
    }
}
