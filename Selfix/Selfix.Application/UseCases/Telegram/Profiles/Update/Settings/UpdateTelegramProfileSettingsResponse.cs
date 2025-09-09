using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.Settings;

public sealed record UpdateTelegramProfileSettingsResponse(TelegramProfileDto Profile);