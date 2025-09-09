using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;

namespace Selfix.Domain.Entities.Jobs.Specifications;

public sealed record NewJobSpecification(JobData Data, DateTimeOffset CurrentTime, Id<User, Ulid> UserId);