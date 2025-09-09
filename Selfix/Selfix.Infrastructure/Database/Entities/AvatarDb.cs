using System.Text.Json;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class AvatarDb
{
    public required Ulid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string OSLoraFilePath { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }

    public required Ulid UserId { get; set; }
    public UserDb? User { get; set; }

    public UserDb? ActiveForUser { get; set; }
}