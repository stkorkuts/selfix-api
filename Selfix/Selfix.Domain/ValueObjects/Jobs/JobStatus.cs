using Selfix.Shared;

namespace Selfix.Domain.ValueObjects.Jobs;

public sealed record JobStatus(JobStatusEnum Value, DateTimeOffset UpdatedAt)
{
    public bool IsCompleted => Value is JobStatusEnum.Succeeded or JobStatusEnum.Failed;
}