using LanguageExt;

namespace Selfix.Domain.ValueObjects.Common;

public sealed class Notes
{
    private const uint MIN_LENGTH = 4;
    private const uint MAX_LENGTH = 2048;

    private readonly LimitedString _value;

    private Notes(LimitedString value)
    {
        _value = value;
    }

    public static Fin<Notes> From(string value)
    {
        return
            from val in LimitedString.From(value, MIN_LENGTH, MAX_LENGTH)
            select new Notes(val);
    }

    public static implicit operator LimitedString(Notes str)
    {
        return str._value;
    }

    public static implicit operator string(Notes str)
    {
        return str._value;
    }
    
    public override string ToString()
    {
        return this;
    }
}