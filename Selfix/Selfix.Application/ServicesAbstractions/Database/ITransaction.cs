using LanguageExt;

namespace Selfix.Application.ServicesAbstractions.Database;

public interface ITransaction
{
    IO<Unit> Commit(CancellationToken cancellationToken);
    IO<Unit> Rollback(CancellationToken cancellationToken);
}