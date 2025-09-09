using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Infrastructure.Telegram.Widgets.Account.Info;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.Start;

internal sealed class OnboardingStartWidget : IWidget<OnboardingStartWidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly AssetsSettings _assetsSettings;

    public OnboardingStartWidget(ITelegramBotClient client, IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _assetsSettings = assetsOptions.Value;
    }

    public IO<Unit> Show(OnboardingStartWidgetContext context, CancellationToken cancellationToken)
    {
        return
            from _1 in context.User.HasPayments switch
            {
                false => HandleStartBeforePayment(context.Profile.Id, cancellationToken),
                true => HandleStartAfterPayment(context, cancellationToken)
            }
            select Unit.Default;
    }

    private IO<Unit> HandleStartBeforePayment(ChatId chatId, CancellationToken cancellationToken)
    {
        var markup = GetPanelMarkup();
        return
            from _2 in _client.SendPhoto(chatId, _assetsSettings.Images.PreviewImageUrl,
                "Привет! Я <b>Селфикс ИИ</b> — нейросеть, которая создает изображения с твоими чертами лица в любых образах на основе твоих фотографий!",
                ParseMode.Html, replyMarkup: markup, cancellationToken: cancellationToken).ToIOUnit()
            from tutorialCallbackData in CallbackSerializer.Serialize(new TutorialCallbackData { Step = 0 }).ToIO()
            from payCallbackData in CallbackSerializer.Serialize(new ShowProductsCallbackData { IsFirstPayment = true}).ToIO()
            let inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Инструкция", tutorialCallbackData))
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Оплатить", payCallbackData))
            from _3 in _client.SendMessage(chatId,
                "Если хочешь сначала разобраться, как всё устроено, нажми «Инструкция»; а если будешь сразу творить — выбирай «Оплатить»",
                replyMarkup: inlineMarkup, cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private IO<Unit> HandleStartAfterPayment(OnboardingStartWidgetContext context, CancellationToken cancellationToken)
    {
        var markup = GetPanelMarkup();
        return
            from _1 in _client.SendPhoto(context.Profile.Id, _assetsSettings.Images.PreviewImageUrl,
                "Привет! Я <b>Селфикс ИИ</b> — нейросеть, которая создает изображения с твоими чертами лица в любых образах на основе твоих фотографий!",
                ParseMode.Html, replyMarkup: markup, cancellationToken: cancellationToken).ToIOUnit()
            from _2 in new AccountInfoWidget(_client).Show(new AccountInfoWidgetContext(context.Profile, context.User),
                cancellationToken)
            select Unit.Default;
    }

    private static ReplyMarkup GetPanelMarkup()
    {
        return new ReplyKeyboardMarkup(true)
            .AddNewRow(TelegramConstants.GENERATE_PREDEFINED_COMMAND_ID,
                TelegramConstants.GENERATE_ANY_COMMAND_ID)
            .AddNewRow(TelegramConstants.AVATARS_COMMAND_ID, TelegramConstants.ACCOUNT_COMMAND_ID)
            .AddNewRow(TelegramConstants.SETTINGS_COMMAND_ID, TelegramConstants.HELP_COMMAND_ID);
    }
}