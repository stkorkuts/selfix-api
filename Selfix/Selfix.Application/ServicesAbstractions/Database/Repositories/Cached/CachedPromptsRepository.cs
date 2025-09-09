using LanguageExt;
using Selfix.Application.ServicesAbstractions.Caching;
using Selfix.Domain.Entities.Prompts;
using Selfix.Domain.ValueObjects.Cache;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared.Extensions;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;

public class CachedPromptsRepository : IPromptsRepository
{
    private readonly ICachingService _cachingService;
    private readonly IPromptsRepository _promptsRepository;

    public CachedPromptsRepository(IPromptsRepository promptsRepository, ICachingService cachingService)
    {
        _promptsRepository = promptsRepository;
        _cachingService = cachingService;
    }

    public OptionT<IO, Prompt> GetById(Id<Prompt, Ulid> id, CancellationToken cancellationToken)
    {
        return _cachingService.Fetch(id, _promptsRepository.GetById(id, cancellationToken), cancellationToken);
    }

    public IO<Iterable<Prompt>> Get(PromptsFilter filter, CancellationToken cancellationToken)
    {
        return _promptsRepository.Get(filter, cancellationToken);
    }

    public IO<uint> Count(CancellationToken cancellationToken)
    {
        return
            from cacheKey in CacheKey.From("Prompts_Count").ToIO()
            from result in _cachingService.Fetch(cacheKey, _promptsRepository.Count(cancellationToken),
                cancellationToken)
            select result;
    }
}