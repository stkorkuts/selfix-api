using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Prompts;

public sealed class PromptText
{
    private const uint MIN_LENGTH = 8;
    private const uint MAX_LENGTH = 8192;

    private readonly LimitedString _value;

    private PromptText(LimitedString value)
    {
        _value = value;
    }

    public static Fin<PromptText> From(string value)
    {
        return
            from limitedString in LimitedString.From(value, MIN_LENGTH, MAX_LENGTH)
            select new PromptText(limitedString);
    }

    public static implicit operator LimitedString(PromptText str)
    {
        return str._value;
    }

    public static implicit operator string(PromptText str)
    {
        return str._value;
    }

    public override string ToString()
    {
        return this;
    }
}