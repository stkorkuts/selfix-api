using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;

namespace Selfix.Shared.Extensions;

public static class IOExtensions
{
    public static IO<TA> InterceptFail<TA, TB>(this IO<TA> io, Func<Error, IO<TB>> f)
    {
        return io.IfFail(
            err => f(err).Match(
                _ => IO<TA>.Fail(err),
                nestedErr => IO<TA>.Fail(err + nestedErr)
            ).Flatten()
        );
    }
    
    public static IO<TA> InterceptFail<TA>(this IO<TA> io, Action<Error> f)
    {
        return io.IfFail(err =>
        {
            f(err);
            return IO<TA>.Fail(err);
        });
    }
    
    public static IO<A> TapOnFail<A, B>(this IO<A> io, Func<Error, IO<B>> func) =>
        io.IfFail(error => func(error)
            .Bind(_ => IO.fail<A>(error))
            .IfFail(innerError => IO.fail<A>(innerError + error)));
    
    public static IO<Unit> IgnoreF<T>(this IO<T> io) => io.Kind().IgnoreF().As();
    
    public static IO<T> WithLogging<T>(this IO<T> io, Action? before, Action? onSuccess, Action<Error>? onError) =>
        from _1 in IO.lift(() => before?.Invoke())
        from result in io.TapOnFail(err => IO.lift(() => onError?.Invoke(err)))
        from _2 in IO.lift(() => onSuccess?.Invoke())
        select result;
}