using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Promocodes;

public sealed class AlphanumericString
{
    private const uint DEFAULT_LENGTH = 32;

    private readonly LimitedString _value;

    private AlphanumericString(LimitedString value)
    {
        _value = value;
    }

    public static AlphanumericString New(uint length = DEFAULT_LENGTH)
    {
        var value = GetRandomBase62EncodedString().PadLeft((int)length, '0')[..(int)length];
        return new AlphanumericString(LimitedString.From(value, length, length).ThrowIfFail());
    }

    private static string GetRandomBase62EncodedString()
    {
        var bytes = new byte[48]; // 48 bytes = 384 bits
        RandomNumberGenerator.Fill(bytes);

        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var dividend = new BigInteger(bytes, isUnsigned: true);
        var builder = new StringBuilder();
        var divisor = new BigInteger(62);

        while (dividend > 0)
        {
            dividend = BigInteger.DivRem(dividend, divisor, out var remainder);
            builder.Insert(0, chars[(int)remainder]);
        }

        return builder.ToString();
    }

    public static Fin<AlphanumericString> From(string value)
    {
        if (!IsBase62String(value)) return Error.New("Invalid promocode value, should be base62 string");
        return
            from limitedString in LimitedString.From(value, DEFAULT_LENGTH, DEFAULT_LENGTH)
            select new AlphanumericString(limitedString);
    }

    private static bool IsBase62String(string value)
    {
        return value.All(c => c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z');
    }

    public static implicit operator LimitedString(AlphanumericString str)
    {
        return str._value;
    }

    public static implicit operator string(AlphanumericString str)
    {
        return str._value;
    }
    
    public override string ToString()
    {
        return this;
    }
}