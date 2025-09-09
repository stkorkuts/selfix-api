using System.Linq.Expressions;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.Promocodes.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Promocodes;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class PromocodesRepository : IPromocodesRepository
{
    private readonly SelfixDbContext _context;

    public PromocodesRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public OptionT<IO, Promocode> GetById(Id<Promocode, Ulid> id, CancellationToken cancellationToken) => Get(p => p.Id == id, cancellationToken);

    public OptionT<IO, Promocode> GetByCode(AlphanumericString code, CancellationToken cancellationToken) => Get(p => p.Code == code, cancellationToken);

    private OptionT<IO, Promocode> Get(Expression<Func<PromocodeDb, bool>> filter, CancellationToken cancellationToken)
    {
        return IO<Option<Promocode>>.LiftAsync(async () =>
        {
            var result = await _context.Promocodes
                .Where(filter)
                .Select(p => new { Promocode = p, IsPaid = p.Order!.Status == OrderStatusEnum.Confirmed })
                .FirstOrDefaultAsync(cancellationToken);
            return result is null ? Option<Promocode>.None : FromDb(result.Promocode, result.IsPaid).ThrowIfFail();
        });
    }    

    public IO<Unit> Save(Promocode promocode, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var existingPromocode =
                await _context.Promocodes.AsTracking()
                    .FirstOrDefaultAsync(p => p.Id == promocode.Id, cancellationToken);
            var usedByUserId = promocode.UsedByUserId.Match<Ulid?>(val => val, () => null);
            if (existingPromocode is not null)
            {
                existingPromocode.Code = promocode.Code;
                existingPromocode.UsedByUserId = usedByUserId;
            }
            else
            {
                var newPromocode = new PromocodeDb
                {
                    Id = promocode.Id,
                    Code = promocode.Code,
                    UsedByUserId = usedByUserId,
                    ProductId = promocode.ProductId
                };
                _context.Add(newPromocode);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private static Fin<Promocode> FromDb(PromocodeDb promocodeDb, bool isPaid)
    {
        var id = Id<Promocode, Ulid>.FromSafe(promocodeDb.Id);
        var productId = Id<Product, Ulid>.FromSafe(promocodeDb.ProductId);
        var usedBy = promocodeDb.UsedByUserId.HasValue
            ? Id<User, Ulid>.FromSafe(promocodeDb.UsedByUserId.Value)
            : Option<Id<User, Ulid>>.None;
        return
            from code in AlphanumericString.From(promocodeDb.Code)
            from promocode in Promocode.Restore(new RestorePromocodeSpecification(id, productId, code, usedBy, isPaid))
            select promocode;
    }
}