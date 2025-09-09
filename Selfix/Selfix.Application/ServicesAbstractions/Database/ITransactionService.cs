using LanguageExt;

namespace Selfix.Application.ServicesAbstractions.Database;

public interface ITransactionService
{
    IO<ITransaction> BeginTransaction(CancellationToken cancellationToken);
}