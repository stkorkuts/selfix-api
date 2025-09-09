using LanguageExt;

namespace Selfix.Application.UseCases.Telegram.Profiles.Create;

public sealed record CreateIfNotCreatedTelegramProfileRequest(
    long TelegramProfileId,
    Option<long> InvitedByTelegramProfileId);