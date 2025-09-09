using LanguageExt;
using Selfix.Shared.Extensions;

namespace Selfix.Application.ServicesAbstractions.Database;

internal static class TransactionExtensions
{
    public static IO<T> Run<T>(this ITransactionService service, IO<T> io, CancellationToken cancellationToken)
    {
        return
            from transaction in service.BeginTransaction(cancellationToken)
            from result in io.InterceptFail(_ => transaction.Rollback(cancellationToken))
            from _1 in transaction.Commit(cancellationToken)
            select result;
    }
}