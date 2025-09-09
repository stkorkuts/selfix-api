using Selfix.Shared;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.State;

public sealed record UpdateTelegramProfileStateRequest(long TelegramProfileId, TelegramProfileStateEnum TargetState);