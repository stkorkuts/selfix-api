using System.Text.Json;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class ImageDb
{
    public required Ulid Id { get; set; }
    public required string OSFilePath { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public required Ulid UserId { get; set; }
    public UserDb? User { get; set; }
}