using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.Entities.Avatars.Specifications;

public sealed record RestoreAvatarSpecification(
    Id<Avatar, Ulid> Id,
    Id<User, Ulid> UserId,
    AvatarName Name,
    AvatarDescription Description,
    OSFilePath FilePath,
    DateTimeOffset CreatedAt);