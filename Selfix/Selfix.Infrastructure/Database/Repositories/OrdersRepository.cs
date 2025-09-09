using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Orders.Specifications;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Domain.ValueObjects.Orders.Products;
using Selfix.Domain.ValueObjects.Orders.Promocodes;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Infrastructure.Database.JsonDocumentSchema.Order;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class OrdersRepository : IOrdersRepository
{
    private readonly SelfixDbContext _context;

    public OrdersRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public OptionT<IO, Order> GetById(Id<Order, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<Order>>.LiftAsync(async () =>
        {
            var orderDb = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
            return orderDb is null ? Option<Order>.None : FromDb(orderDb).ThrowIfFail();
        });
    }

    public IO<Unit> Save(Order order, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var notes = order.Notes.Match<string?>(val => val, () => null);
            var paymentData = order.PaymentData
                .Map(val => JsonSerializer.SerializeToDocument(
                    new OrderPaymentDataJsonDocumentSchema(),
                    DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)).ValueUnsafe();
            var existingOrder = await _context.Orders.AsTracking()
                .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);
            if (existingOrder is not null)
            {
                existingOrder.Status = order.Status.Value;
                existingOrder.UpdatedAt = order.Status.UpdatedAt;
                existingOrder.PaymentData = paymentData;
                existingOrder.Notes = notes;
            }
            else
            {
                var newOrder = new OrderDb
                {
                    Id = order.Id,
                    Status = order.Status.Value,
                    Type = order.Data switch
                    {
                        ProductOrderData => OrderTypeEnum.Product,
                        PromocodeOrderData => OrderTypeEnum.Promocode,
                        _ => throw new Exception("Unsupported order type")
                    },
                    PaymentData = paymentData,
                    Notes = notes,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.Status.UpdatedAt,
                    UserId = order.UserId,
                    ProductId = order.Data is ProductOrderData productOrderData
                        ? (Ulid)productOrderData.ProductId
                        : null,
                    PromocodeId = order.Data is PromocodeOrderData promocodeOrderData
                        ? (Ulid)promocodeOrderData.PromocodeId
                        : null
                };
                _context.Add(newOrder);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private static Fin<Order> FromDb(OrderDb orderDb)
    {
        var id = Id<Order, Ulid>.FromSafe(orderDb.Id);
        var userId = Id<User, Ulid>.FromSafe(orderDb.UserId);
        var status = new OrderStatus(orderDb.Status, orderDb.UpdatedAt);
        var paymentData =
            (orderDb.PaymentData?.Deserialize<OrderPaymentDataJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS) 
             ?? Option<OrderPaymentDataJsonDocumentSchema>.None)
            .Map(_ => new OrderPaymentData());
        return
            from data in GetOrderData(orderDb)
            from notes in string.IsNullOrWhiteSpace(orderDb.Notes)
                ? Option<Notes>.None
                : Notes.From(orderDb.Notes).Map(Option<Notes>.Some)
            from order in Order.Restore(new RestoreOrderSpecification(
                id, userId, status, data, paymentData, notes, orderDb.CreatedAt))
            select order;
    }

    private static Fin<OrderData> GetOrderData(OrderDb orderDb)
    {
        return orderDb.Type switch
        {
            OrderTypeEnum.Product => new ProductOrderData(Id<Product, Ulid>.FromSafe(orderDb.ProductId!.Value)),
            OrderTypeEnum.Promocode => new PromocodeOrderData(Id<Promocode, Ulid>.FromSafe(orderDb.PromocodeId!.Value)),
            _ => Error.New("Unsupported order type")
        };
    }
}