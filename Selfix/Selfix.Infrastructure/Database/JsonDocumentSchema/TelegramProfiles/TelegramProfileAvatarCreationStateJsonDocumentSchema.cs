namespace Selfix.Infrastructure.Database.JsonDocumentSchema.TelegramProfile;

internal sealed record TelegramProfileAvatarCreationStateJsonDocumentSchema(
    string[] FileIds,
    string? AvatarName);