using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Orders.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Shared;

namespace Selfix.Domain.Entities.Orders;

public sealed class Order
{
    private Order(Id<Order, Ulid> id, Id<User, Ulid> userId, OrderStatus status, OrderData data,
        Option<OrderPaymentData> paymentData, Option<Notes> notes, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Status = status;
        Data = data;
        PaymentData = paymentData;
        Notes = notes;
        CreatedAt = createdAt;
    }

    public Id<Order, Ulid> Id { get; }
    public Id<User, Ulid> UserId { get; }
    public OrderStatus Status { get; private set; }
    public OrderData Data { get; }
    public Option<OrderPaymentData> PaymentData { get; private set; }
    public Option<Notes> Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static Fin<Order> New(NewOrderSpecification specs)
    {
        var id = Id<Order, Ulid>.FromSafe(Ulid.NewUlid());
        var status = new OrderStatus(OrderStatusEnum.Processing, specs.CurrentTime);
        var notes = Option<Notes>.None;
        return new Order(id, specs.UserId, status, specs.Data, Option<OrderPaymentData>.None, notes, specs.CurrentTime);
    }

    public static Fin<Order> Restore(RestoreOrderSpecification specs)
    {
        return new Order(specs.Id, specs.UserId, specs.Status, specs.Data, specs.PaymentData, specs.Notes, specs.CreatedAt);
    }

    public Fin<Unit> SetPaymentData(OrderPaymentData paymentData)
    {
        return PaymentData.Match(
            _ => Error.New("Payment data already set"),
            () =>
            {
                PaymentData = paymentData;
                return Fin<Unit>.Succ(Unit.Default);
            });
    }

    public Fin<Unit> ChangeStatus(OrderStatus newStatus)
    {
        if (Status.IsCompleted)
            return Error.New("Order is already completed.");
        if (newStatus.Value is OrderStatusEnum.Created)
            return Error.New("You can not change status to created");

        Status = newStatus;
        return Unit.Default;
    }

    public Fin<Unit> AddNotes(Notes notes)
    {
        return Notes.Match(
            currentNotes =>
                from combinedNotes in ValueObjects.Common.Notes.From((string)currentNotes + "\n" + (string)notes)
                let _1 = Notes = combinedNotes
                select Unit.Default,
            () =>
                from _1 in Fin<Unit>.Succ(Unit.Default)
                let _2 = Notes = notes
                select Unit.Default
        );
    }
}