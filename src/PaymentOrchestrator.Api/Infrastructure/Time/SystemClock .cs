using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Api.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
