using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Quotas;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Application.UseCases.Telegram.Profiles.Get;

public sealed record GetTelegramProfileWithUserResponse(TelegramProfileDto Profile, UserDto User, UserQuotasDto Quotas);