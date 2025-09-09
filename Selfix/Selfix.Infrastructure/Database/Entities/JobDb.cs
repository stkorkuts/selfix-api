using System.Text.Json;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class JobDb
{
    public required Ulid Id { get; set; }
    public required JobTypeEnum Type { get; set; }
    public required JobStatusEnum Status { get; set; }

    public required JsonDocument Input { get; set; }
    public JsonDocument? Output { get; set; }

    public required string? Notes { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public required Ulid UserId { get; set; }
    public UserDb? User { get; set; }
}