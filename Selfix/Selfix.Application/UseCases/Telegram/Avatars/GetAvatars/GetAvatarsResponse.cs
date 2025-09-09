using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;

namespace Selfix.Application.UseCases.Telegram.Avatars.GetAvatars;

public sealed record GetAvatarsResponse(Iterable<AvatarDto> Avatars);