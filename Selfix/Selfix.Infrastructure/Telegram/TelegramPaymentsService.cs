using System.Globalization;
using System.Text.Json;
using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Payments;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Infrastructure.Telegram.Payments.Schema;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types.Payments;

namespace Selfix.Infrastructure.Payments;

internal sealed class TelegramPaymentsService : ITelegramPaymentsService
{
    private readonly ITelegramBotClient _client;
    private readonly TelegramBotSettings _telegramBotPaymentsSettings;

    public TelegramPaymentsService(ITelegramBotClient client, IOptions<TelegramBotSettings> settings)
    {
        _client = client;
        _telegramBotPaymentsSettings = settings.Value;
    }

    public IO<OrderPaymentData> CreatePayment(TelegramProfileDto profile, OrderDto order, ProductDto product,
        CancellationToken cancellationToken)
    {
        return IO<OrderPaymentData>.LiftAsync(async () =>
        {
            var result = await _client.SendInvoice(
                chatId: profile.Id,
                title: product.Name,
                description: product.Name,
                payload: JsonSerializer.Serialize(new OrderPayload
                {
                    OrderId = order.Id,
                }),
                currency: "RUB",
                prices:
                [
                    new LabeledPrice
                    {
                        Label = product.Name,
                        Amount = (int)(product.Price * 100)
                    }
                ],
                providerToken: _telegramBotPaymentsSettings.Payments.YooKassa
                    .PaymentProviderToken,
                needEmail: true,
                sendEmailToProvider: true,
                providerData: JsonSerializer.Serialize(new ProviderData
                {
                    Receipt = new ProviderDataReceipt
                    {
                        Items =
                        [
                            new ProviderDataReceiptItem
                            {
                                Description = product.Name,
                                Quantity = 1,
                                Amount = new ProviderDataReceiptItemAmount
                                {
                                    Currency = "RUB",
                                    Value = (int)product.Price
                                },
                                VatCode = 1,
                                PaymentMode = "full_payment",
                                PaymentSubject = "commodity"
                            }
                        ]
                    }
                }),
                cancellationToken: cancellationToken
            );
            return new OrderPaymentData();
        });
    }
}