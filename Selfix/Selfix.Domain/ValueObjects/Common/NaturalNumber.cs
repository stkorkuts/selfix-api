using System.Globalization;
using LanguageExt;

namespace Selfix.Domain.ValueObjects.Common;

public sealed class NaturalNumber
{
    private const uint MIN_VALUE = 1;

    private readonly uint _value;

    private NaturalNumber(uint value)
    {
        _value = value;
    }

    public static Fin<NaturalNumber> From(uint value)
    {
        return value < MIN_VALUE
            ? Fin<NaturalNumber>.Fail($"Number must start from {MIN_VALUE}")
            : new NaturalNumber(value);
    }

    public static implicit operator uint(NaturalNumber num)
    {
        return num._value;
    }
    
    public static implicit operator int(NaturalNumber num)
    {
        return num._value <= int.MaxValue ? (int)num._value : int.MaxValue;
    }
    
    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }
}