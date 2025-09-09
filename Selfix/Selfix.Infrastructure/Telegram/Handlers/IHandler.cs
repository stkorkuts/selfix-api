using LanguageExt;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal interface IHandler
{
    IO<bool> TryHandle(Update update, CancellationToken cancellationToken);
}