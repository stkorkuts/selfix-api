using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selfix.Application.UseCases.Orders.Confirm;
using Selfix.Application.UseCases.Orders.Create;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.Cancel;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.Confirm;
using Selfix.Application.UseCases.Telegram.Avatars.SetActive;
using Selfix.Application.UseCases.Telegram.Images.GenerateByPredefinedPrompt;
using Selfix.Application.UseCases.Telegram.Products.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Update.Settings;
using Selfix.Application.UseCases.Telegram.Profiles.Update.State;
using Selfix.Application.UseCases.Telegram.Prompts.Get;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Infrastructure.Telegram.Widgets;
using Selfix.Infrastructure.Telegram.Widgets.Account.InviteUser;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Activated;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.AskForAvatarName;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Cancelled;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Started;
using Selfix.Infrastructure.Telegram.Widgets.Images.Generation.Starting;
using Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;
using Selfix.Infrastructure.Telegram.Widgets.Onboarding.CustomPromptTutorial;
using Selfix.Infrastructure.Telegram.Widgets.Onboarding.Tutorial;
using Selfix.Infrastructure.Telegram.Widgets.Products.List;
using Selfix.Infrastructure.Telegram.Widgets.Prompts.Page;
using Selfix.Infrastructure.Telegram.Widgets.Settings.AspectRatio;
using Selfix.Infrastructure.Telegram.Widgets.Settings.ImagesPerRequest;
using Selfix.Infrastructure.Telegram.Widgets.Settings.Panel;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Unit = LanguageExt.Unit;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class CallbackHandler : IHandler
{
    private readonly IAvatarCreationCancelUseCase _avatarCreationCancelUseCase;
    private readonly IAvatarCreationConfirmUseCase _avatarCreationConfirmUseCase;
    private readonly ITelegramBotClient _client;
    private readonly ICreateOrderUseCase _createOrderUseCase;
    private readonly IProcessPredefinedPromptUseCase _processPredefinedPromptUseCase;
    private readonly IGetProductsUseCase _getProductsUseCase;
    private readonly IGetPromptsUseCase _getPromptsUseCase;
    private readonly IGetTelegramProfileWithUserUseCase _getTelegramProfileWithUserUseCase;
    private readonly ISetActiveAvatarUseCase _setActiveAvatarUseCase;
    private readonly IUpdateTelegramProfileSettingsUseCase _updateTelegramProfileSettingsUseCase;
    private readonly IUpdateTelegramProfileStateUseCase _updateTelegramProfileStateUseCase;
    private readonly IConfirmOrderUseCase _confirmOrderUseCase;
    private readonly IOptions<AssetsSettings> _assetsOptions;
    private readonly IOptions<TelegramBotSettings> _telegramBotOptions;

    public CallbackHandler(ITelegramBotClient client,
        IGetTelegramProfileWithUserUseCase getTelegramProfileWithUserUseCase, IGetPromptsUseCase getPromptsUseCase,
        IProcessPredefinedPromptUseCase processPredefinedPromptUseCase,
        ISetActiveAvatarUseCase setActiveAvatarUseCase, IAvatarCreationCancelUseCase avatarCreationCancelUseCase,
        IGetProductsUseCase getProductsUseCase, ICreateOrderUseCase createOrderUseCase,
        IAvatarCreationConfirmUseCase avatarCreationConfirmUseCase,
        IUpdateTelegramProfileSettingsUseCase updateTelegramProfileSettingsUseCase,
        IUpdateTelegramProfileStateUseCase updateTelegramProfileStateUseCase,
        IConfirmOrderUseCase confirmOrderUseCase,
        IOptions<AssetsSettings> assetsOptions, IOptions<TelegramBotSettings> telegramBotOptions)
    {
        _client = client;
        _getTelegramProfileWithUserUseCase = getTelegramProfileWithUserUseCase;
        _getPromptsUseCase = getPromptsUseCase;
        _processPredefinedPromptUseCase = processPredefinedPromptUseCase;
        _setActiveAvatarUseCase = setActiveAvatarUseCase;
        _avatarCreationCancelUseCase = avatarCreationCancelUseCase;
        _getProductsUseCase = getProductsUseCase;
        _createOrderUseCase = createOrderUseCase;
        _updateTelegramProfileStateUseCase = updateTelegramProfileStateUseCase;
        _confirmOrderUseCase = confirmOrderUseCase;
        _assetsOptions = assetsOptions;
        _telegramBotOptions = telegramBotOptions;
        _avatarCreationConfirmUseCase = avatarCreationConfirmUseCase;
        _updateTelegramProfileSettingsUseCase = updateTelegramProfileSettingsUseCase;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(update.CallbackQuery?.Data)) return IO<bool>.Pure(false);
        if (update.CallbackQuery.Message is null) return IO<bool>.Pure(false);

        return (
                from callbackData in CallbackSerializer.Deserialize(update.CallbackQuery.Data).ToIO()
                from _1 in callbackData switch
                {
                    TutorialCallbackData cd => HandleTutorialCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    PredefinedPromptsSwitchPageCallbackData cd => HandlePredefinedPromptsSwitchPageCallback(cd,
                        update.CallbackQuery.From.Id, update.CallbackQuery.Message, cancellationToken),
                    PredefinedPromptsGenerateImageCallbackData cd => HandlePredefinedPromptsGenerateImageCallback(cd,
                        update.CallbackQuery.From.Id, update.CallbackQuery.Message, cancellationToken),
                    CreateAvatarCallbackData cd => HandleCreateAvatarCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    SetActiveAvatarCallbackData cd => HandleSetActiveAvatarCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    CancelAvatarCreationCallbackData cd => HandleCancelAvatarCreationCallback(cd,
                        update.CallbackQuery.From.Id, cancellationToken),
                    ShowProductsCallbackData cd => HandleShowProductsCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    BuyPackageCallbackData cd => HandleBuyPackageCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    SettingsShowAspectRatioOptionsCallbackData cd => HandleShowAspectRatioOptionsCallback(cd,
                        update.CallbackQuery.From.Id,
                        cancellationToken),
                    SettingsShowImagesPerRequestOptionsCallbackData cd => HandleShowImagesPerRequestOptionsCallback(cd,
                        update.CallbackQuery.From.Id,
                        cancellationToken),
                    InviteUserCallbackData cd => HandleInviteUserCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    BuyPromocodeCallbackData cd => HandleBuyPromocodeCallback(cd, update.CallbackQuery.From.Id,
                        cancellationToken),
                    SettingsSetAspectRatioCallbackData cd => HandleSettingsSetAspectRatioCallback(cd,
                        update.CallbackQuery.From.Id, cancellationToken),
                    SettingsSetImagesPerRequestCallbackData cd => HandleSettingsSetImagesPerRequestCallback(cd,
                        update.CallbackQuery.From.Id, cancellationToken),
                    ConfirmAvatarCreationCallbackData cd => HandleConfirmAvatarCreationCallback(cd,
                        update.CallbackQuery.From.Id, cancellationToken),
                    CustomPromptTutorialCallbackData cd => HandleCustomPromptTutorialCallback(cd,
                        update.CallbackQuery.From.Id, cancellationToken),
                    _ => IO<Unit>.Fail(
                        new InvalidOperationException($"There is no handler for callback: {update.CallbackQuery.Data}"))
                }
                from _2 in _client.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken)
                    .ToIO()
                select Unit.Default
            )
            .InterceptFail(_ =>
                from _1 in _client.SendMessage(update.CallbackQuery.From.Id,
                        "Ошибка обработки команды, пожалуйста, попробуйте позже", cancellationToken: cancellationToken)
                    .ToIO()
                from _2 in _client.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken)
                    .ToIO()
                select Unit.Default)
            .Map(_ => true);
    }

    private IO<Unit> HandleTutorialCallback(TutorialCallbackData callbackData, long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in new TutorialWidget(_client, _assetsOptions).Show(
                new TutorialWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User,
                    callbackData.Step), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandlePredefinedPromptsSwitchPageCallback(
        PredefinedPromptsSwitchPageCallbackData predefinedPromptsSwitchPageCallbackData, long telegramProfileId,
        Message message, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from publicPromptsResponse in _getPromptsUseCase.Execute(
                new GetPromptsRequest(TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE,
                    predefinedPromptsSwitchPageCallbackData.TargetPageIndex),
                cancellationToken)
            from _1 in new PromptsPageWidget(_client)
                .Show(new PromptsPageWidgetContext(telegramProfileResponse.Profile,
                        publicPromptsResponse.PromptsPage,
                        message),
                    cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandlePredefinedPromptsGenerateImageCallback(
        PredefinedPromptsGenerateImageCallbackData predefinedPromptsGenerateImageCallbackData, long telegramProfileId,
        Message message, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            let imagesLeftAfterOperation = (int)telegramProfileResponse.User.AvailableImageGenerations -
                                           telegramProfileResponse.Profile.Settings.ImagesToGeneratePerRequest
            from _1 in telegramProfileResponse.Quotas.CanGenerateImages switch
            {
                false => new SimpleNotificationWidget(_client).Show(
                    new SimpleNotificationWidgetContext(telegramProfileResponse.Profile, 
                        "Похоже что вы уже генерируете несколько изображений, подождите пожалуйста пока они будут готовы перед созданием новых"),
                    cancellationToken),
                true => imagesLeftAfterOperation switch
                {
                    < 0 => from productsResponse in _getProductsUseCase.Execute(
                            new GetProductsRequest(telegramProfileId),
                            cancellationToken)
                        from _1 in new ProductsListWidget(_client, _assetsOptions).Show(
                            new ProductsListWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User,
                                productsResponse.Products, false),
                            cancellationToken)
                        select Unit.Default,
                    _ => from generateImageResponse in _processPredefinedPromptUseCase.Execute(
                            new ProcessPredefinedPromptRequest(telegramProfileId,
                                predefinedPromptsGenerateImageCallbackData.PromptId), cancellationToken)
                        from publicPromptsResponse in _getPromptsUseCase.Execute(
                            new GetPromptsRequest(TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE,
                                predefinedPromptsGenerateImageCallbackData.PageIndex), cancellationToken)
                        from _2 in new ImageGenerationStartedWidget(_client)
                            .Show(new ImageGenerationStartedWidgetContext(
                                    telegramProfileResponse.Profile, (message, publicPromptsResponse.PromptsPage)),
                                cancellationToken)
                        select Unit.Default
                }
            }
            select Unit.Default;
    }

    private IO<Unit> HandleCreateAvatarCallback(CreateAvatarCallbackData _,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from _1 in telegramProfileResponse.Quotas.CanGenerateAvatars switch
            {
                false => new SimpleNotificationWidget(_client).Show(
                    new SimpleNotificationWidgetContext(telegramProfileResponse.Profile, 
                        "Похоже что вы уже генерируете аватар, подождите пожалуйста пока он будет готов перед созданием нового"),
                    cancellationToken),
                true => telegramProfileResponse.User.AvailableAvatarGenerations switch
                {
                    > 0 => from _1 in _updateTelegramProfileStateUseCase.Execute(
                            new UpdateTelegramProfileStateRequest(telegramProfileId,
                                TelegramProfileStateEnum.AvatarCreation),
                            cancellationToken)
                        from _2 in new AvatarCreationAskForAvatarNameWidget(_client).Show(
                            new WidgetContext(telegramProfileResponse.Profile), cancellationToken)
                        select Unit.Default,
                    _ => from productsResponse in _getProductsUseCase.Execute(
                            new GetProductsRequest(telegramProfileId),
                            cancellationToken)
                        from _1 in new ProductsListWidget(_client, _assetsOptions).Show(
                            new ProductsListWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User,
                                productsResponse.Products, false),
                            cancellationToken)
                        select Unit.Default
                }
            }
            select Unit.Default;
    }

    private IO<Unit> HandleSetActiveAvatarCallback(SetActiveAvatarCallbackData callbackData,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from activeAvatarResponse in _setActiveAvatarUseCase.Execute(
                new SetActiveAvatarRequest(telegramProfileResponse.User.Id, callbackData.AvatarId), cancellationToken)
            from _2 in new AvatarActivatedWidget(_client).Show(
                new AvatarActivatedWidgetContext(telegramProfileResponse.Profile, activeAvatarResponse.Avatar),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleCancelAvatarCreationCallback(CancelAvatarCreationCallbackData _,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from _1 in _avatarCreationCancelUseCase.Execute(new AvatarCreationCancelRequest(telegramProfileId),
                cancellationToken)
            from _2 in new AvatarCreationCancelledWidget(_client).Show(
                new AvatarCreationCancelledWidgetContext(telegramProfileResponse.Profile), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleShowProductsCallback(ShowProductsCallbackData _, long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from productsResponse in _getProductsUseCase.Execute(
                new GetProductsRequest(telegramProfileId), cancellationToken)
            from _1 in new ProductsListWidget(_client, _assetsOptions).Show(
                new ProductsListWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User,
                    productsResponse.Products, false), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleBuyPackageCallback(BuyPackageCallbackData callbackData,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from getTelegramProfileWithUserResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId),
                cancellationToken)
            from orderResponse in _createOrderUseCase.Execute(
                new CreateOrderRequest(getTelegramProfileWithUserResponse.Profile.Id, callbackData.PackageId,
                    callbackData.IsPromocode),
                cancellationToken)
            from _1 in getTelegramProfileWithUserResponse.User.IsAdmin switch
            {
                false => IO<Unit>.Pure(Unit.Default),
                true => _confirmOrderUseCase.Execute(new ConfirmOrderRequest(orderResponse.Order.Id), cancellationToken)
                    .IgnoreF()
            }
            select Unit.Default;
    }

    private IO<Unit> HandleShowAspectRatioOptionsCallback(SettingsShowAspectRatioOptionsCallbackData _,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in new SettingsAspectRatioWidget(_client).Show(new WidgetContext(telegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleShowImagesPerRequestOptionsCallback(SettingsShowImagesPerRequestOptionsCallbackData _,
        long telegramProfileId, CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in new SettingsImagesPerRequestWidget(_client).Show(
                new WidgetContext(telegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleInviteUserCallback(InviteUserCallbackData _, long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in new AccountInviteUserWidget(_client, _telegramBotOptions).Show(
                new WidgetContext(telegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleBuyPromocodeCallback(BuyPromocodeCallbackData _, long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from productsResponse in _getProductsUseCase.Execute(new GetProductsRequest(telegramProfileId),
                cancellationToken)
            from _1 in new ProductsListWidget(_client, _assetsOptions).Show(
                new ProductsListWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User,
                    productsResponse.Products, true), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleSettingsSetAspectRatioCallback(SettingsSetAspectRatioCallbackData callback,
        long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from updateTelegramProfileResponse in _updateTelegramProfileSettingsUseCase.Execute(
                new UpdateTelegramProfileSettingsRequest(telegramProfileId,
                    Option<uint>.None, callback.NewRatio), cancellationToken)
            from _1 in new SettingsPanelWidget(_client).Show(new WidgetContext(updateTelegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleSettingsSetImagesPerRequestCallback(SettingsSetImagesPerRequestCallbackData callback,
        long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from updateTelegramProfileResponse in _updateTelegramProfileSettingsUseCase.Execute(
                new UpdateTelegramProfileSettingsRequest(telegramProfileId,
                    callback.ImagesPerRequest, Option<ImageAspectRatioEnum>.None), cancellationToken)
            from _1 in new SettingsPanelWidget(_client).Show(new WidgetContext(updateTelegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleConfirmAvatarCreationCallback(ConfirmAvatarCreationCallbackData _,
        long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in _avatarCreationConfirmUseCase.Execute(
                new AvatarCreationConfirmRequest(telegramProfileId), cancellationToken)
            from _2 in new AvatarCreationStartedWidget(_client).Show(
                new AvatarCreationStartedWidgetContext(telegramProfileResponse.Profile),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleCustomPromptTutorialCallback(CustomPromptTutorialCallbackData _, long telegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(telegramProfileId), cancellationToken)
            from _1 in new CustomPromptTutorialWidget(_client, _assetsOptions).Show(
                new CustomPromptTutorialWidgetContext(telegramProfileResponse.Profile, telegramProfileResponse.User),
                cancellationToken)
            select Unit.Default;
    }
}