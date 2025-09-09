using LanguageExt;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IOrdersRepository
{
    OptionT<IO, Order> GetById(Id<Order, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(Order order, CancellationToken cancellationToken);
}