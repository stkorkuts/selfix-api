using Selfix.Domain.Entities.Jobs;

namespace Selfix.Application.Dtos.Jobs;

internal static class JobMappingExtensions
{
    public static JobDto ToDto(this Job job)
    {
        return new JobDto(job.Id, job.Data);
    }
}