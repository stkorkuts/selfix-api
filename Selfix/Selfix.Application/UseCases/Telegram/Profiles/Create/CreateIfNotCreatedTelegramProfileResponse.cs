using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Application.UseCases.Telegram.Profiles.Create;

public sealed record CreateIfNotCreatedTelegramProfileResponse(TelegramProfileDto Profile, UserDto User);