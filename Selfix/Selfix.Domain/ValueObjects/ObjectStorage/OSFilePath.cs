using System.Text.RegularExpressions;
using LanguageExt;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Promocodes;

namespace Selfix.Domain.ValueObjects.ObjectStorage;

public sealed partial class OSFilePath
{
    private const uint MIN_LENGTH = 4;
    private const uint MAX_LENGTH = 256;

    private readonly LimitedString _value;

    private OSFilePath(LimitedString value)
    {
        _value = value;
    }

    public static Fin<OSFilePath> From(string path)
    {
        if (!OSFilePathRegex().IsMatch(path))
            return Fin<OSFilePath>.Fail("Invalid file path");

        return
            from value in LimitedString.From(path, MIN_LENGTH, MAX_LENGTH)
            select new OSFilePath(value);
    }

    public static Fin<OSFilePath> New(string prefix, string extension)
    {
        var name = AlphanumericString.New();
        var path = $"{prefix}{name}.{extension}";
        return From(path);
    }

    public static implicit operator LimitedString(OSFilePath name)
    {
        return name._value;
    }

    public static implicit operator string(OSFilePath name)
    {
        return name._value;
    }
    
    public override string ToString()
    {
        return this;
    }

    [GeneratedRegex(@"([A-Za-z0-9_]+)(/[A-Za-z0-9_]+)*(\.\w+)?")]
    private static partial Regex OSFilePathRegex();
}