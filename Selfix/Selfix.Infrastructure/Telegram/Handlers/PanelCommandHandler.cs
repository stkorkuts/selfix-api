using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos.Users;
using Selfix.Application.UseCases.Telegram.Avatars.GetAvatars;
using Selfix.Application.UseCases.Telegram.Products.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Update.State;
using Selfix.Application.UseCases.Telegram.Prompts.Get;
using Selfix.Infrastructure.Telegram.Widgets;
using Selfix.Infrastructure.Telegram.Widgets.Account.Info;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.List;
using Selfix.Infrastructure.Telegram.Widgets.Help;
using Selfix.Infrastructure.Telegram.Widgets.Images.Generation.AskForCustomPrompt;
using Selfix.Infrastructure.Telegram.Widgets.Products.List;
using Selfix.Infrastructure.Telegram.Widgets.Prompts.Page;
using Selfix.Infrastructure.Telegram.Widgets.Settings.Panel;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Unit = LanguageExt.Unit;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class PanelCommandHandler : IHandler
{
    private readonly ITelegramBotClient _client;
    private readonly IGetAvatarsUseCase _getAvatarsUseCase;
    private readonly IGetPromptsUseCase _getPromptsUseCase;
    private readonly IGetTelegramProfileWithUserUseCase _getTelegramProfileWithUserUseCase;
    private readonly ILogger _logger;
    private readonly TelegramBotSettings _settings;
    private readonly IUpdateTelegramProfileStateUseCase _updateTelegramProfileStateUseCase;
    private readonly IGetProductsUseCase _getProductsUseCase;
    private readonly IOptions<AssetsSettings> _assetsOptions;

    public PanelCommandHandler(ITelegramBotClient client, ILogger logger,
        IOptions<TelegramBotSettings> settings, IGetTelegramProfileWithUserUseCase getTelegramProfileWithUserUseCase,
        IGetPromptsUseCase getPromptsUseCase, IGetAvatarsUseCase getAvatarsUseCase,
        IUpdateTelegramProfileStateUseCase updateTelegramProfileStateUseCase, IGetProductsUseCase getProductsUseCase,
        IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _logger = logger;
        _settings = settings.Value;
        _getTelegramProfileWithUserUseCase = getTelegramProfileWithUserUseCase;
        _getPromptsUseCase = getPromptsUseCase;
        _getAvatarsUseCase = getAvatarsUseCase;
        _updateTelegramProfileStateUseCase = updateTelegramProfileStateUseCase;
        _getProductsUseCase = getProductsUseCase;
        _assetsOptions = assetsOptions;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is null) return IO<bool>.Pure(false);

        var command = update.Message.Text.Trim(' ');

        if (!TelegramConstants.PANEL_COMMANDS_SET.Contains(command)) return IO<bool>.Pure(false);

        _logger.Information("Start processing {Command} command for chat: {ChatId}", command,
            update.Message.Chat.Id);

        return
            (from _1 in command switch
                {
                    TelegramConstants.GENERATE_PREDEFINED_COMMAND_ID => HandleGeneratePredefinedCommand(
                        update.Message.Chat.Id,
                        cancellationToken),
                    TelegramConstants.GENERATE_ANY_COMMAND_ID => HandleGenerateAnyCommand(update.Message.Chat.Id,
                        cancellationToken),
                    TelegramConstants.AVATARS_COMMAND_ID => HandleAvatarsCommand(update.Message.Chat.Id,
                        cancellationToken),
                    TelegramConstants.ACCOUNT_COMMAND_ID => HandleAccountCommand(update.Message.Chat.Id,
                        cancellationToken),
                    TelegramConstants.SETTINGS_COMMAND_ID =>
                        HandleSettingsCommand(update.Message.Chat.Id, cancellationToken),
                    TelegramConstants.HELP_COMMAND_ID => HandleHelpCommand(update.Message.Chat.Id, cancellationToken),
                    _ => IO<Unit>.Fail(
                        new InvalidOperationException($"There is no handler for command: {command}"))
                }
                select true)
            .InterceptFail(_ => NotifyIfError(update, _client, cancellationToken));
    }

    private IO<Unit> HandleGeneratePredefinedCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in EnsureUserCanGenerate(telegramProfileResponse,
                from promptsResponse in _getPromptsUseCase.Execute(
                    new GetPromptsRequest(TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE, 0), cancellationToken)
                from _1 in new PromptsPageWidget(_client).Show(
                    new PromptsPageWidgetContext(telegramProfileResponse.Profile, promptsResponse.PromptsPage,
                        Option<Message>.None), cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleGenerateAnyCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in EnsureUserCanGenerate(telegramProfileResponse,
                from updateTelegramProfileStateResponse in _updateTelegramProfileStateUseCase.Execute(
                    new UpdateTelegramProfileStateRequest(telegramProfileId,
                        TelegramProfileStateEnum.ImageGeneration),
                    cancellationToken)
                from _1 in new ImageGenerationAskForCustomPromptWidget(_client).Show(
                    new WidgetContext(updateTelegramProfileStateResponse.Profile),
                    cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> EnsureUserCanGenerate(GetTelegramProfileWithUserResponse response,
        IO<Unit> generateIO, CancellationToken cancellationToken)
    {
        var user = response.User;
        var settings = response.Profile.Settings;
        return user switch
        {
            { HasActiveAvatar: false } =>
                from avatarsResponse in _getAvatarsUseCase.Execute(
                    new GetAvatarsRequest(response.User.Id),
                    cancellationToken)
                from _1 in new AvatarsListWidget(_client).Show(
                    new AvatarsListWidgetContext(response.Profile, avatarsResponse.Avatars),
                    cancellationToken)
                select Unit.Default,
            not null when user.AvailableImageGenerations < settings.ImagesToGeneratePerRequest =>
                from productsResponse in _getProductsUseCase.Execute(new GetProductsRequest(response.Profile.Id),
                    cancellationToken)
                from _1 in new ProductsListWidget(_client, _assetsOptions).Show(
                    new ProductsListWidgetContext(response.Profile, response.User,
                        productsResponse.Products, false), cancellationToken)
                select Unit.Default,
            _ => generateIO,
        };
    }

    private IO<Unit> HandleAvatarsCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from avatarsResponse in _getAvatarsUseCase.Execute(
                new GetAvatarsRequest(telegramProfileResponse.User.Id),
                cancellationToken)
            from _1 in new AvatarsListWidget(_client).Show(
                new AvatarsListWidgetContext(telegramProfileResponse.Profile, avatarsResponse.Avatars),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleAccountCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from _1 in new AccountInfoWidget(_client).Show(
                new AccountInfoWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleSettingsCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from _1 in new SettingsPanelWidget(_client).Show(new WidgetContext(telegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleHelpCommand(long telegramProfileId, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(_settings.HelpAccountUrl, UriKind.Absolute, out var helpCenterUrl))
            return Error.New("Help account url is invalid url string");

        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from _1 in new HelpWidget(_client).Show(new HelpWidgetContext(
                telegramProfileResponse.Profile, helpCenterUrl
            ), cancellationToken)
            select Unit.Default;
    }

    private static IO<bool> NotifyIfError(Update update, ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        const string failMessage = "Ошибка при обработке сообщения. Пожалуйста, попробуйте позже";
        return
            from _1 in update.Type switch
            {
                UpdateType.Message when update.Message is not null =>
                    botClient.SendMessage(update.Message.Chat.Id, failMessage,
                        cancellationToken: cancellationToken).ToIOUnit(),
                UpdateType.CallbackQuery when update.CallbackQuery is not null =>
                    from _1 in botClient.SendMessage(update.CallbackQuery.From.Id, failMessage,
                        cancellationToken: cancellationToken).ToIOUnit()
                    from _2 in botClient.AnswerCallbackQuery(update.CallbackQuery.Id,
                        cancellationToken: cancellationToken).ToIO()
                    select Unit.Default,
                _ => IO<Unit>.Pure(Unit.Default)
            }
            select false;
    }
}