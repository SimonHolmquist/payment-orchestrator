using System.ComponentModel.DataAnnotations;

namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public sealed class IdempotencyKey
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string ClientId { get; set; } = default!;

    [MaxLength(200)]
    public string Key { get; set; } = default!;

    [MaxLength(200)]
    public string OperationName { get; set; } = default!;

    [MaxLength(64)]
    public string RequestHash { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public int? ResponseStatusCode { get; set; }
    [MaxLength(100)]
    public string? ResponseContentType { get; set; }
    public string? ResponseBody { get; set; }
}
