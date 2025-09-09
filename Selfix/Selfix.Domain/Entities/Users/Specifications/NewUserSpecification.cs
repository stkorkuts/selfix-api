using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.Entities.Users.Specifications;

public sealed record NewUserSpecification(Option<Id<User, Ulid>> InvitedById, DateTimeOffset CreatedAt);