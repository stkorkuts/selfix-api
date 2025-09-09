using LanguageExt;
using Selfix.Domain.ValueObjects.Cache;

namespace Selfix.Application.ServicesAbstractions.Caching;

public interface ICachingService
{
    OptionT<IO, T> GetValue<T>(CacheKey key, CancellationToken cancellationToken);
    IO<Unit> SetValue<T>(CacheKey key, T value, CancellationToken cancellationToken);
}