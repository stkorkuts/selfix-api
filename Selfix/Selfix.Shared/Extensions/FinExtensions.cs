using LanguageExt;

namespace Selfix.Shared.Extensions;

public static class FinExtensions
{
    public static IO<T> ToIO<T>(this Fin<T> fin)
    {
        return fin.Match(IO<T>.Pure, IO<T>.Fail);
    }
}