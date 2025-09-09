namespace Selfix.Application.UseCases.Orders.Confirm;

public sealed record ConfirmOrderRequest(Ulid OrderId);