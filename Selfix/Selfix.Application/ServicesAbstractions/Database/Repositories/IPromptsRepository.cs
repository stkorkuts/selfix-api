using LanguageExt;
using Selfix.Domain.Entities.Prompts;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IPromptsRepository
{
    OptionT<IO, Prompt> GetById(Id<Prompt, Ulid> id, CancellationToken cancellationToken);
    IO<Iterable<Prompt>> Get(PromptsFilter filter, CancellationToken cancellationToken);
    IO<uint> Count(CancellationToken cancellationToken);
}