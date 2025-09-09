using Selfix.Domain.Entities.Users;

namespace Selfix.Application.Dtos.Users;

internal static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.AvailableAvatarGenerations,
            user.AvailableImageGenerations,
            user.HasPayments,
            user.ActiveAvatarId.IsSome,
            user.IsAdmin
        );
    }
}