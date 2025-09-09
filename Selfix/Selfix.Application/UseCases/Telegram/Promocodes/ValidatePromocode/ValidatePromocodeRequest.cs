namespace Selfix.Application.UseCases.Telegram.Promocodes.ValidatePromocode;

public sealed record ValidatePromocodeRequest(long TelegramProfileId, string Promocode);