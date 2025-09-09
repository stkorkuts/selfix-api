using System.Text.Json;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class TelegramProfileDb
{
    public required long TelegramId { get; set; }
    public required TelegramProfileStateEnum ProfileState { get; set; }
    public required JsonDocument Settings { get; set; }
    public required JsonDocument StateData { get; set; }

    public required Ulid UserId { get; set; }
    public UserDb? User { get; set; }
}