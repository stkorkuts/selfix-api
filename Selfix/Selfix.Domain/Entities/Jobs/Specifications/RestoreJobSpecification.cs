using LanguageExt;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;

namespace Selfix.Domain.Entities.Jobs.Specifications;

public sealed record RestoreJobSpecification(
    Id<Job, Ulid> Id,
    Id<User, Ulid> UserId,
    JobStatus Status,
    JobData Data,
    Option<Notes> Notes,
    DateTimeOffset CreatedAt);