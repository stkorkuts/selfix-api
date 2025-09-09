namespace Selfix.Domain.ValueObjects.Common;

public sealed class Id<T, TId> : IEquatable<Id<T, TId>> where TId : IEquatable<TId>
{
    private readonly TId _value;

    private Id(TId value)
    {
        _value = value;
    }

    public bool Equals(Id<T, TId>? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || _value.Equals(other._value);
    }

    public static Id<T, TId> FromSafe(TId value)
    {
        return new Id<T, TId>(value);
    }

    public override string ToString()
    {
        return $"Identifier of {typeof(T).Name} ({_value.ToString()})";
    }

    public static implicit operator TId(Id<T, TId> id)
    {
        return id._value;
    }
    
    public static implicit operator string(Id<T, TId> id)
    {
        return id._value.ToString() ?? "Undefined";
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is Id<T, TId> other && Equals(other));
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }
}