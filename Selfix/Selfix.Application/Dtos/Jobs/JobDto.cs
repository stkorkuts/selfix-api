using Selfix.Domain.ValueObjects.Jobs;

namespace Selfix.Application.Dtos.Jobs;

public sealed record JobDto(Ulid Id, JobData Data);