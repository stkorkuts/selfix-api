namespace Selfix.Application.UseCases.Orders.Create;

public sealed record CreateOrderRequest(long TelegramProfileId, Ulid ProductId, bool IsPromocode);