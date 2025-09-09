namespace Selfix.Application.UseCases.Orders.Cancel;

public sealed record CancelOrderRequest(Ulid OrderId);