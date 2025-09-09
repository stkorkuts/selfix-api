using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;

namespace Selfix.Application.UseCases.Telegram.Avatars.SetActive;

public sealed record SetActiveAvatarResponse(AvatarDto Avatar);