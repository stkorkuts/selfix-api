using LanguageExt;

namespace Selfix.Infrastructure.Telegram.Widgets;

internal interface IWidget<in T> where T : WidgetContext
{
    IO<Unit> Show(T context, CancellationToken cancellationToken);
}