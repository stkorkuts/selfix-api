using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Promocodes;

namespace Selfix.Domain.Entities.Promocodes;

public sealed class Promocode
{
    private Promocode(Id<Promocode, Ulid> id, AlphanumericString code, Id<Product, Ulid> productId,
        Option<Id<User, Ulid>> usedByUserId, bool isPaid)
    {
        Id = id;
        Code = code;
        ProductId = productId;
        UsedByUserId = usedByUserId;
        IsPaid = isPaid;
    }

    public Id<Promocode, Ulid> Id { get; private set; }
    public AlphanumericString Code { get; private set; }
    public Id<Product, Ulid> ProductId { get; private set; }
    public Option<Id<User, Ulid>> UsedByUserId { get; private set; }
    public bool IsPaid { get; private set; }

    public static Fin<Promocode> New(NewPromocodeSpecification specs)
    {
        var id = Id<Promocode, Ulid>.FromSafe(Ulid.NewUlid());
        var code = AlphanumericString.New();
        return new Promocode(id, code, specs.ProductId, Option<Id<User, Ulid>>.None, false);
    }

    public static Fin<Promocode> Restore(RestorePromocodeSpecification specs)
    {
        return new Promocode(specs.Id, specs.Code, specs.ProductId, specs.UsedBy, specs.IsPaid);
    }

    public Fin<Unit> Use(Id<User, Ulid> userId)
    {
        if (UsedByUserId.IsSome) return Error.New("Promocode already used");
        UsedByUserId = userId;
        return Unit.Default;
    }
}