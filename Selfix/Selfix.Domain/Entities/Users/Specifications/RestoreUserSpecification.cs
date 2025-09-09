using LanguageExt;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.Entities.Users.Specifications;

public sealed record RestoreUserSpecification(Id<User, Ulid> Id, Option<Id<User, Ulid>> InvitedByUserId, uint AvailableAvatarGenerations,
    uint AvailableImageGenerations, bool HasPayments, Option<Id<Avatar, Ulid>> ActiveAvatarId, DateTimeOffset CreatedAt,
    bool IsAdmin);