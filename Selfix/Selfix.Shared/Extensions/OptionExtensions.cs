using LanguageExt;
using LanguageExt.Common;

namespace Selfix.Shared.Extensions;

public static class OptionExtensions
{
    public static IO<T> ToIOFailIfNone<T>(this Option<T> option, Error error)
    {
        return option.ToIO(() => IO<T>.Fail(error));
    }

    public static IO<T> ToIOFailIfNone<T>(this OptionT<IO, T> transformer, Error error)
    {
        return transformer.Run().Bind(o => o.ToIOFailIfNone(error)).As();
    }

    public static IO<T> ToIO<T>(this Option<T> option, Func<IO<T>> ifNone)
    {
        return option.Match(IO<T>.Pure, ifNone);
    }
}