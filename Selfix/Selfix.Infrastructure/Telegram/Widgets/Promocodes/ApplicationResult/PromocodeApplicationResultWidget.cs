using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Promocodes.ApplicationResult;

internal sealed class PromocodeApplicationResultWidget : IWidget<PromocodeApplicationResultWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public PromocodeApplicationResultWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(PromocodeApplicationResultWidgetContext context, CancellationToken cancellationToken)
    {
        return context.IsApplied switch
        {
            true => _client.SendMessage(context.Profile.Id, "Промокод успешно применён!",
                cancellationToken: cancellationToken).ToIOUnit(),
            false => _client.SendMessage(context.Profile.Id, "Промокод уже использован",
                cancellationToken: cancellationToken).ToIOUnit()
        };
    }
}