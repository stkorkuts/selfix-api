using System.Text.RegularExpressions;
using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.ObjectStorage;

public sealed partial class OSBucketName
{
    private const uint MIN_LENGTH = 4;
    private const uint MAX_LENGTH = 64;

    private readonly LimitedString _value;

    private OSBucketName(LimitedString value)
    {
        _value = value;
    }

    public static Fin<OSBucketName> From(string bucketName)
    {
        if (!OSBucketNameRegex().IsMatch(bucketName))
            return Fin<OSBucketName>.Fail("Invalid bucket name");

        return
            from value in LimitedString.From(bucketName, MIN_LENGTH, MAX_LENGTH)
            select new OSBucketName(value);
    }

    public static implicit operator LimitedString(OSBucketName name)
    {
        return name._value;
    }

    public static implicit operator string(OSBucketName name)
    {
        return name._value;
    }
    
    public override string ToString()
    {
        return this;
    }

    [GeneratedRegex("[A-Za-z0-9-_]+")]
    private static partial Regex OSBucketNameRegex();
}