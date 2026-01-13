using System.ComponentModel.DataAnnotations;

namespace PaymentOrchestrator.Infrastructure.Persistence.Entities;

public class IdempotencyKey
{
    [Key]
    [MaxLength(200)]
    public string Key { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string OperationName { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}