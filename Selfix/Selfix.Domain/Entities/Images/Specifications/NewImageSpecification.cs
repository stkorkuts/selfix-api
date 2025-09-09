using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.Entities.Images.Specifications;

public sealed record NewImageSpecification(
    OSFilePath Path,
    DateTimeOffset CreatedAt,
    Id<User, Ulid> UserId);