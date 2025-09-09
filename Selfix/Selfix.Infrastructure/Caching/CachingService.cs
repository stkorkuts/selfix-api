using LanguageExt;
using Selfix.Application.ServicesAbstractions.Caching;
using Selfix.Domain.ValueObjects.Cache;

namespace Selfix.Infrastructure.Caching;

internal sealed class CachingService : ICachingService
{
    public OptionT<IO, T> GetValue<T>(CacheKey key, CancellationToken cancellationToken)
    {
        return OptionT<IO, T>.None;
    }

    public IO<Unit> SetValue<T>(CacheKey key, T value, CancellationToken cancellationToken)
    {
        return IO<Unit>.Pure(Unit.Default);
    }
}