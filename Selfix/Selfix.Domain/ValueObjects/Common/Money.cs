using System.Globalization;
using LanguageExt;

namespace Selfix.Domain.ValueObjects.Common;

public sealed class Money
{
    private readonly decimal _amount;

    private Money(decimal amount)
    {
        _amount = amount;
    }

    public static Fin<Money> From(decimal amount)
    {
        return amount < 0
            ? Fin<Money>.Fail("Amount must be greater than or equal to 0")
            : new Money(amount);
    }

    public static implicit operator decimal(Money money)
    {
        return money._amount;
    }
    
    public override string ToString()
    {
        return _amount.ToString(CultureInfo.InvariantCulture);
    }
}