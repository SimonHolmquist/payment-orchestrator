namespace PaymentOrchestrator.Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{name} is required.");

        return value.Trim();
    }

    public static decimal Positive(decimal value, string name)
    {
        if (value <= 0)
            throw new DomainException($"{name} must be > 0.");

        return value;
    }

    public static decimal NonNegative(decimal value, string name)
    {
        if (value < 0)
            throw new DomainException($"{name} must be >= 0.");

        return value;
    }
}