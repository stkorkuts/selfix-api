using LanguageExt;
using LanguageExt.Common;

namespace Selfix.Domain.ValueObjects.Common;

public sealed class LimitedString
{
    private readonly string _value;

    private LimitedString(string value)
    {
        _value = value;
    }

    public static Fin<LimitedString> From(string value, uint min = 1, uint max = uint.MaxValue, bool truncate = false)
    {
        if(string.IsNullOrEmpty(value)) return Error.New("Limited string value cannot be null or empty.");
        if (truncate)
        {
            var clampedMax = Math.Clamp(max > int.MaxValue ? int.MaxValue : (int)max, 0, value.Length);
            return new LimitedString(value[..clampedMax]);
        }
        if (value.Length > max || value.Length < min)
            return Error.New($"String length must be between {min} and {max} characters");

        return new LimitedString(value);
    }

    public static implicit operator string(LimitedString str)
    {
        return str._value;
    }
    
    public override string ToString()
    {
        return this;
    }
}