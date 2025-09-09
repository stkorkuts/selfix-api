using LanguageExt;
using Selfix.Domain.ValueObjects.Cache;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared.Extensions;

namespace Selfix.Application.ServicesAbstractions.Caching;

public static class CachingServiceExtensions
{
    public static OptionT<IO, T> Fetch<T>(this ICachingService cachingService, CacheKey key,
        OptionT<IO, T> io, CancellationToken cancellationToken)
    {
        return
            cachingService
                .GetValue<T>(key, cancellationToken)
                .BindNone(() =>
                    from item in io
                    from _2 in cachingService.SetValue<T>(key, item, cancellationToken)
                    select item);
    }

    public static IO<T2> Save<T1, T2>(this ICachingService cachingService, CacheKey key, T1 value,
        IO<T2> io, CancellationToken cancellationToken)
    {
        return
            from result in io
            from _2 in cachingService.SetValue(key, value, cancellationToken).IfFail(Unit.Default)
            select result;
    }

    public static OptionT<IO, T> Fetch<T, TId>(this ICachingService cachingService, Id<T, TId> id,
        OptionT<IO, T> io, CancellationToken cancellationToken) where TId : IEquatable<TId>
    {
        return
            from cacheKey in CacheKey.From(id).ToIO()
            from result in cachingService
                .GetValue<T>(cacheKey, cancellationToken)
                .BindNone(() =>
                    from item in io
                    from _2 in cachingService.SetValue<T>(cacheKey, item, cancellationToken)
                    select item)
            select result;
    }

    public static IO<T2> Save<T1, T2, TId>(this ICachingService cachingService, Id<T1, TId> id, T1 value,
        IO<T2> io, CancellationToken cancellationToken) where TId : IEquatable<TId>
    {
        return
            from cacheKey in CacheKey.From(id).ToIO()
            from result in io
            from _2 in cachingService.SetValue(cacheKey, value, cancellationToken).IfFail(Unit.Default)
            select result;
    }

    public static IO<T> Fetch<T>(this ICachingService cachingService, CacheKey key, IO<T> io,
        CancellationToken cancellationToken)
    {
        return
            cachingService
                .GetValue<T>(key, cancellationToken)
                .Run()
                .Bind(opt =>
                    opt.Match(
                        IO<T>.Pure,
                        () => from item in io
                            from _2 in cachingService.SetValue<T>(key, item, cancellationToken)
                            select item
                    )
                ).As();
    }
}