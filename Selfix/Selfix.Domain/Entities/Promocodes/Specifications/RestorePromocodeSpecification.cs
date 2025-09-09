using LanguageExt;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Promocodes;

namespace Selfix.Domain.Entities.Promocodes.Specifications;

public sealed record RestorePromocodeSpecification(Id<Promocode, Ulid> Id, Id<Product, Ulid> ProductId, AlphanumericString Code, Option<Id<User, Ulid>> UsedBy, bool IsPaid);