using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Avatars;

public sealed class AvatarDescription
{
    private const uint MIN_LENGTH = 1;
    private const uint MAX_LENGTH = 8192;

    private readonly LimitedString _value;

    private AvatarDescription(LimitedString value)
    {
        _value = value;
    }

    public static Fin<AvatarDescription> From(string value)
    {
        return
            from valueLimited in LimitedString.From(value, MIN_LENGTH, MAX_LENGTH, true)
            select new AvatarDescription(valueLimited);
    }

    public static implicit operator LimitedString(AvatarDescription name)
    {
        return name._value;
    }

    public static implicit operator string(AvatarDescription name)
    {
        return name._value;
    }

    public override string ToString()
    {
        return this;
    }
}