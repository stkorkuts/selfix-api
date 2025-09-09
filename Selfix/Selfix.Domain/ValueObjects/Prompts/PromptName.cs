using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Prompts;

public sealed class PromptName
{
    private const uint MIN_LENGTH = 4;
    private const uint MAX_LENGTH = 256;

    private readonly LimitedString _value;

    private PromptName(LimitedString value)
    {
        _value = value;
    }

    public static Fin<PromptName> From(string value)
    {
        return
            from limitedString in LimitedString.From(value, MIN_LENGTH, MAX_LENGTH)
            select new PromptName(limitedString);
    }

    public static implicit operator LimitedString(PromptName str)
    {
        return str._value;
    }

    public static implicit operator string(PromptName str)
    {
        return str._value;
    }

    public override string ToString()
    {
        return this;
    }
}