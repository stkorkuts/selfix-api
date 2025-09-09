using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Selfix.Application.UseCases.Statistics.Application;
using Selfix.Application.UseCases.Telegram.Profiles.Create;
using Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;
using Selfix.Infrastructure.Telegram.Widgets.Onboarding.Start;
using Selfix.Shared.Settings;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class MainCommandHandler : IHandler
{
    private const string START_COMMAND = "/start";
    private const string APP_STATISTICS_COMMAND = "/appstats";
    private static readonly string[] KNOWN_COMMANDS = [START_COMMAND, APP_STATISTICS_COMMAND];

    private readonly ITelegramBotClient _client;
    private readonly ICreateIfNotCreatedTelegramProfileUseCase _createIfNotCreatedTelegramProfileUseCase;
    private readonly IOptions<AssetsSettings> _assetsOptions;
    private readonly IGetApplicationStatisticsUseCase _getApplicationStatisticsUseCase;
    private readonly ILogger _logger;

    public MainCommandHandler(ITelegramBotClient client, ILogger logger,
        ICreateIfNotCreatedTelegramProfileUseCase createIfNotCreatedTelegramProfileUseCase,
        IOptions<AssetsSettings> assetsOptions, IGetApplicationStatisticsUseCase getApplicationStatisticsUseCase)
    {
        _client = client;
        _logger = logger;
        _createIfNotCreatedTelegramProfileUseCase = createIfNotCreatedTelegramProfileUseCase;
        _assetsOptions = assetsOptions;
        _getApplicationStatisticsUseCase = getApplicationStatisticsUseCase;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is null) return IO<bool>.Pure(false);

        var commandWithArg = update.Message.Text.Trim(' ').Split(' ');
        var command = commandWithArg[0];
        var arg = commandWithArg.Length > 1 ? commandWithArg[1] : string.Empty;

        if (!KNOWN_COMMANDS.Contains(command)) return IO<bool>.Pure(false);

        _logger.Information("Start handling command: {Command} with args: {Arg} for chat: {ChatId}", command, arg,
            update.Message.Chat.Id);

        var invitedBy = !string.IsNullOrWhiteSpace(arg) && long.TryParse(arg, out var invitedById)
            ? invitedById
            : Option<long>.None;

        return
            from telegramProfileResponse in _createIfNotCreatedTelegramProfileUseCase.Execute(
                new CreateIfNotCreatedTelegramProfileRequest(update.Message.Chat.Id, invitedBy),
                cancellationToken)
            from _1 in command switch
            {
                START_COMMAND => HandleStartCommand(telegramProfileResponse, cancellationToken),
                APP_STATISTICS_COMMAND => HandleAppStatisticsCommand(telegramProfileResponse, cancellationToken),
                _ => HandleUnknownCommand(command)
            }
            select true;
    }

    private IO<Unit> HandleStartCommand(CreateIfNotCreatedTelegramProfileResponse telegramProfileResponse, CancellationToken cancellationToken)
    {
        return new OnboardingStartWidget(_client, _assetsOptions)
            .Show(new OnboardingStartWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User),
                cancellationToken);
    }

    private IO<Unit> HandleAppStatisticsCommand(CreateIfNotCreatedTelegramProfileResponse telegramProfileResponse, CancellationToken cancellationToken)
    {
        return
            from appStatisticsResponse in _getApplicationStatisticsUseCase.Execute(new GetApplicationStatisticsRequest(telegramProfileResponse.User.Id), cancellationToken)
            let statisticsContent = $"""
Сейчас генерируется {appStatisticsResponse.Statistics.CurrentlyCreatingAvatars} аватаров и {appStatisticsResponse.Statistics.CurrentlyGeneratingPromptsAndImages} изображений.
За сегодня сделано {appStatisticsResponse.Statistics.TodayOrdersCount} заказов
"""
            from _1 in new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(telegramProfileResponse.Profile, statisticsContent), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleUnknownCommand(string command)
    {
        _logger.Error("There is no handler for command: {Command}", command);

        return Error.New(new InvalidOperationException($"There is no handler for command: {command}"));
    }
}