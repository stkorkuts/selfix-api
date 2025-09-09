namespace Selfix.Application.UseCases.Telegram.Promocodes.TryApplyPromocode;

public sealed record TryApplyPromocodeRequest(long TelegramProfileId, string Promocode);