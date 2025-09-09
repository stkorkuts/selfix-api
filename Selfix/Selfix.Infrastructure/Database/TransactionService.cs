using LanguageExt;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Shared.Extensions;

namespace Selfix.Infrastructure.Database;

internal sealed class TransactionService : ITransactionService
{
    private readonly SelfixDbContext _dbContext;

    public TransactionService(SelfixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IO<ITransaction> BeginTransaction(CancellationToken cancellationToken)
    {
        return from dbContextTransaction in _dbContext.Database.BeginTransactionAsync(cancellationToken).ToIO()
            select Transaction.Create(dbContextTransaction);
    }
}