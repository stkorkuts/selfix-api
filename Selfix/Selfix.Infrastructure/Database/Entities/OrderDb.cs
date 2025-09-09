using System.Text.Json;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class OrderDb
{
    public required Ulid Id { get; set; }
    public required OrderStatusEnum Status { get; set; }
    public required OrderTypeEnum Type { get; set; }

    public required JsonDocument? PaymentData { get; set; }

    public string? Notes { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public required Ulid UserId { get; set; }
    public UserDb? User { get; set; }

    public Ulid? ProductId { get; set; }
    public ProductDb? Product { get; set; }

    public Ulid? PromocodeId { get; set; }
    public PromocodeDb? Promocode { get; set; }
}