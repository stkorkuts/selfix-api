using LanguageExt;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Products;

public sealed class ProductName
{
    private const uint MIN_LENGTH = 4;
    private const uint MAX_LENGTH = 128;

    private readonly LimitedString _value;

    private ProductName(LimitedString value)
    {
        _value = value;
    }

    public static Fin<ProductName> From(string path)
    {
        return
            from value in LimitedString.From(path, MIN_LENGTH, MAX_LENGTH)
            select new ProductName(value);
    }

    public static implicit operator LimitedString(ProductName name)
    {
        return name._value;
    }

    public static implicit operator string(ProductName name)
    {
        return name._value;
    }
    
    public override string ToString()
    {
        return this;
    }
}