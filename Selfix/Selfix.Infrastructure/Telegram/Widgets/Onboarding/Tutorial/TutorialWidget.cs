using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.Tutorial;

internal sealed class TutorialWidget : IWidget<TutorialWidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly AssetsSettings _assetsSettings;

    public TutorialWidget(ITelegramBotClient client, IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _assetsSettings = assetsOptions.Value;
    }

    public IO<Unit> Show(TutorialWidgetContext context, CancellationToken cancellationToken)
    {
        return context.Step switch
        {
            0 => HandleTutorialEntryPointCommand(context.Profile, cancellationToken),
            1 => HandleTutorialStep1Command(context.Profile, context.User, cancellationToken),
            _ => IO<Unit>.Fail(new NotSupportedException("Unknown tutorial step"))
        };
    }

    private IO<Unit> HandleTutorialEntryPointCommand(TelegramProfileDto profile,
        CancellationToken cancellationToken)
    {
        return
            from callbackData in CallbackSerializer.Serialize(new TutorialCallbackData { Step = 1 }).ToIO()
            let keyboard = new InlineKeyboardMarkup().AddNewRow(InlineKeyboardButton.WithCallbackData(
                "Всё понятно!",
                callbackData))
            let caption = """
                          Селфикс ИИ сканирует твои фото, чтобы уловить твои уникальные черты, и создает невероятно реалистичные изображения, которые не отличишь от снимков на камеру телефона — воплоти в жизнь любую фантазию!

                          Это волшебство! Вот примеры!
                          """
            from _1 in _client.SendPhoto(profile.Id,
                _assetsSettings.Images.PreviewImageUrl,
                caption,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private IO<Unit> HandleTutorialStep1Command(TelegramProfileDto profile, UserDto user,
        CancellationToken cancellationToken)
    {
        return
            from callbackData in CallbackSerializer.Serialize(new ShowProductsCallbackData
                { IsFirstPayment = !user.HasPayments }).ToIO()
            let keyboard = new InlineKeyboardMarkup()
                .AddNewRow(InlineKeyboardButton.WithCallbackData(
                    "А сколько стоит?",
                    callbackData
                ))
            let caption = """
                          С Селфикс ИИ ты сможешь выбрать 2 режима работы:

                          • Образы: Выбирай готовый вариант — например, «космонавт», «свадьба» или «Мальдивы» — и получай реалистичное фото себя в новом образе всего менее чем за минуту!
                          • Режим творца: создавай любой образ по своему текстовому запросу!
                          """
            from _1 in _client.SendPhoto(profile.Id,
                _assetsSettings.Images.PromptImageUrl,
                caption,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}