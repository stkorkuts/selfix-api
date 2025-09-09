namespace Selfix.Application.Dtos.Users;

public sealed record UserDto(
    Ulid Id,
    uint AvailableAvatarGenerations,
    uint AvailableImageGenerations,
    bool HasPayments,
    bool HasActiveAvatar,
    bool IsAdmin);