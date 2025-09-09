using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Selfix.Infrastructure.Database.Extensions;

internal interface IDbEntity<out TId>
{
    TId Id { get; }
}

internal static class SelfixDbContextExtensions
{
    public static IO<Unit> Save<TEntity, TId>(this SelfixDbContext context, DbSet<TEntity> dbSet, TEntity entity, 
        CancellationToken cancellationToken)
        where TEntity : class, IDbEntity<TId>
        where TId : IEquatable<TId> =>
        IO<Unit>.LiftAsync(async () =>
        {
            EntityEntry<TEntity>? entry = dbSet.Local.FindEntry(entity.Id);

            if (entry is null)
            {
                TEntity? existingAvatar =
                    await dbSet.FirstOrDefaultAsync(a => a.Id.Equals(entity.Id), cancellationToken);

                if (existingAvatar is null)
                {
                    context.Add(entity);
                }
                else
                {
                    context.Entry(existingAvatar).CurrentValues.SetValues(entity);
                }
            }
            else
            {
                entry.CurrentValues.SetValues(entity);
            }

            await context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
}