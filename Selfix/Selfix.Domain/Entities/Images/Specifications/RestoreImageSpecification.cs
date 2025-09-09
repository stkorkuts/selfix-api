using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.Entities.Images.Specifications;

public record RestoreImageSpecification(
    Id<Image, Ulid> Id,
    Id<User, Ulid> UserId,
    OSFilePath Path,
    DateTimeOffset CreatedAt
);