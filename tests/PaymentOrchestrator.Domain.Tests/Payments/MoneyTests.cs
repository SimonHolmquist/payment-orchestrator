using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments.ValueObjects;

namespace PaymentOrchestrator.Domain.Tests.Payments;

public class MoneyTests
{
    [Fact]
    public void Of_WithNegativeAmount_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => Money.Of(-1, "USD"));
        Assert.Contains("must be >= 0", ex.Message);
    }

    [Theory]
    [InlineData("US")]    // Corto
    [InlineData("USDT")]  // Largo
    [InlineData("usd")]   // Minúsculas (se normaliza a Mayúsculas, pero probamos validación estricta si aplica)
    [InlineData("123")]   // Numérico
    public void Of_WithInvalidCurrency_ThrowsOrNormalizes(string currency)
    {
        if (currency.Length != 3)
        {
            var ex = Assert.Throws<DomainException>(() => Money.Of(100, currency));
            Assert.Contains("3-letter ISO code", ex.Message);
        }
        else if (currency == "usd")
        {
            // Caso feliz: normalización
            var money = Money.Of(100, currency);
            Assert.Equal("USD", money.Currency);
        }
    }

    [Fact]
    public void Subtract_ResultingInNegative_Throws()
    {
        var m1 = Money.Of(50, "USD");
        var m2 = Money.Of(100, "USD");

        var ex = Assert.Throws<DomainException>(() => m1.Subtract(m2));
        Assert.Contains("negative amount", ex.Message);
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        var usd = Money.Of(50, "USD");
        var eur = Money.Of(50, "EUR");

        var ex = Assert.Throws<DomainException>(() => usd.Add(eur));
        Assert.Contains("Currency mismatch", ex.Message);
    }
}