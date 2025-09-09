using LanguageExt;
using Microsoft.EntityFrameworkCore.Storage;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Shared.Extensions;

namespace Selfix.Infrastructure.Database;

internal sealed class Transaction : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    private Transaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public IO<Unit> Commit(CancellationToken cancellationToken)
    {
        return _transaction.CommitAsync(cancellationToken).ToIO();
    }

    public IO<Unit> Rollback(CancellationToken cancellationToken)
    {
        return _transaction.RollbackAsync(cancellationToken).ToIO();
    }

    public static ITransaction Create(IDbContextTransaction transaction)
    {
        return new Transaction(transaction);
    }
}