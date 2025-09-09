using Selfix.Domain.Entities.Avatars;

namespace Selfix.Application.Dtos.Avatars;

internal static class AvatarMappingExtensions
{
    public static AvatarDto ToDto(this Avatar avatar)
    {
        return new AvatarDto(avatar.Id, avatar.Name);
    }
}