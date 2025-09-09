using System.Text.Json;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class PromptDb
{
    public required Ulid Id { get; set; }
    public required string Name { get; set; }
    public required int NumberInOrder { get; set; }
    public required string Text { get; set; }
}