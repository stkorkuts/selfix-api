using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Cache;

public sealed class CacheKey
{
    private const uint MIN_LENGTH = 3;
    private const uint MAX_LENGTH = 65;

    private readonly LimitedString _value;

    private CacheKey(LimitedString value)
    {
        _value = value;
    }

    public static Fin<CacheKey> From(string value)
    {
        var parts = value.Split("_");
        if (parts.Length is not 2) return Error.New("Cache key must contain two parts separated by underscore");
        if (parts.Any(p => p.Length > 32)) return Error.New("Cache key parts must be less than 32 characters");
        return
            from val in LimitedString.From(value, MIN_LENGTH, MAX_LENGTH)
            select new CacheKey(val);
    }

    public static Fin<CacheKey> From<T, TId>(Id<T, TId> id) where TId : IEquatable<TId>
    {
        return From($"{typeof(T).Name}_{(TId)id}");
    }

    public static implicit operator LimitedString(CacheKey str)
    {
        return str._value;
    }

    public static implicit operator string(CacheKey str)
    {
        return str._value;
    }
    
    public override string ToString()
    {
        return this;
    }
}