using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Avatars;

public sealed class AvatarName
{
    private const uint MIN_NAME_LENGTH = 1;
    private const uint MAX_NAME_LENGTH = 64;

    private readonly LimitedString _value;

    private AvatarName(LimitedString value)
    {
        _value = value;
    }

    public static Fin<AvatarName> From(string value)
    {
        return
            from valueLimited in LimitedString.From(value, MIN_NAME_LENGTH, MAX_NAME_LENGTH)
            select new AvatarName(valueLimited);
    }

    public static implicit operator LimitedString(AvatarName name)
    {
        return name._value;
    }

    public static implicit operator string(AvatarName name)
    {
        return name._value;
    }

    public override string ToString()
    {
        return this;
    }
}