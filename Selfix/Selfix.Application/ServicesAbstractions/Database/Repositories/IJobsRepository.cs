using LanguageExt;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IJobsRepository
{
    OptionT<IO, Job> GetById(Id<Job, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(Job job, CancellationToken cancellationToken);
}