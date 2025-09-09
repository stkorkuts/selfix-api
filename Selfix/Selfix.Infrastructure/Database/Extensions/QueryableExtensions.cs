using System.Linq.Expressions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Shared.Extensions;

namespace Selfix.Infrastructure.Database.Extensions;

public static class QueryableExtensions
{
    public static OptionT<IO, TDomain> GetByPredicate<TDb, TDomain>(
        this IQueryable<TDb> queryable,
        Expression<Func<TDb, bool>> expression,
        Func<TDb, Fin<TDomain>> toDomain,
        CancellationToken cancellationToken) =>
        from dbValue in OptionT<IO, TDb>.LiftIO(
            IO.liftAsync(() => queryable
                .FirstOrDefaultAsync(expression, cancellationToken)
                .Map(Prelude.Optional)))
        from domainValue in toDomain(dbValue).ToIO()
        select domainValue;
}