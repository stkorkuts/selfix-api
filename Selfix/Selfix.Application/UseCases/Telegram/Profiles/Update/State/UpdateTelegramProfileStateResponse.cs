using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.State;

public sealed record UpdateTelegramProfileStateResponse(TelegramProfileDto Profile);