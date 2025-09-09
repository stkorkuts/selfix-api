using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Application.Dtos.Quotas;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.SetName;
using Selfix.Application.UseCases.Telegram.Products.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Get;
using Selfix.Application.UseCases.Telegram.Promocodes.TryApplyPromocode;
using Selfix.Application.UseCases.Telegram.Promocodes.ValidatePromocode;
using Selfix.Application.UseCases.Telegram.Prompts.ProcessRawUserPrompt;
using Selfix.Application.UseCases.Telegram.Prompts.ValidateCustomPrompt;
using Selfix.Infrastructure.Telegram.Widgets;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.AskToLoadImages;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.InvalidName;
using Selfix.Infrastructure.Telegram.Widgets.Images.Generation.Starting;
using Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;
using Selfix.Infrastructure.Telegram.Widgets.Products.List;
using Selfix.Infrastructure.Telegram.Widgets.Promocodes.ApplicationResult;
using Selfix.Infrastructure.Telegram.Widgets.Prompts.Invalid;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Selfix.Shared.Types;
using Telegram.Bot;
using Telegram.Bot.Types;
using Unit = LanguageExt.Unit;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class RegularTextHandler : IHandler
{
    private readonly IAvatarCreationSetAvatarNameUseCase _avatarCreationSetAvatarNameUseCase;
    private readonly ITelegramBotClient _client;
    private readonly IGetProductsUseCase _getProductsUseCase;
    private readonly IGetTelegramProfileWithUserUseCase _getTelegramProfileWithUserUseCase;
    private readonly IProcessRawUserPromptUseCase _processRawUserPromptUseCase;
    private readonly ITryApplyPromocodeUseCase _tryApplyPromocodeUseCase;
    private readonly IOptions<AssetsSettings> _assetsOptions;
    private readonly IValidateCustomPromptUseCase _validateCustomPromptUseCase;
    private readonly IValidatePromocodeUseCase _validatePromocodeUseCase;

    public RegularTextHandler(ITelegramBotClient client, IGetTelegramProfileWithUserUseCase getTelegramProfileUseCase,
        IAvatarCreationSetAvatarNameUseCase avatarCreationSetAvatarNameUseCase,
        IProcessRawUserPromptUseCase processRawUserPromptUseCase,
        IGetProductsUseCase getProductsUseCase, IValidatePromocodeUseCase validatePromocodeUseCase,
        IValidateCustomPromptUseCase validateCustomPromptUseCase, ITryApplyPromocodeUseCase tryApplyPromocodeUseCase,
        IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _getTelegramProfileWithUserUseCase = getTelegramProfileUseCase;
        _avatarCreationSetAvatarNameUseCase = avatarCreationSetAvatarNameUseCase;
        _processRawUserPromptUseCase = processRawUserPromptUseCase;
        _getProductsUseCase = getProductsUseCase;
        _validatePromocodeUseCase = validatePromocodeUseCase;
        _validateCustomPromptUseCase = validateCustomPromptUseCase;
        _tryApplyPromocodeUseCase = tryApplyPromocodeUseCase;
        _assetsOptions = assetsOptions;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is null) return IO<bool>.Pure(false);

        return
            from profileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(update.Message.Chat.Id),
                cancellationToken)
            from promocodeApplicationResult in TryApplyPromocode(update.Message.Text, profileResponse.Profile,
                cancellationToken)
            from result in promocodeApplicationResult switch
            {
                true => IO<bool>.Pure(true),
                false => profileResponse.Profile.ProfileState switch
                {
                    TelegramProfileStateEnum.AvatarCreation => TrySetAvatarName(update.Message.Text,
                        profileResponse.Profile,
                        cancellationToken),
                    TelegramProfileStateEnum.ImageGeneration => TryGenerateImageByPrompt(update.Message.Text,
                        profileResponse.Profile, profileResponse.User, profileResponse.Quotas, cancellationToken),
                    _ => IO<bool>.Pure(false)
                },
            }
            select result;
    }

    private IO<bool> TrySetAvatarName(string name, TelegramProfileDto profile,
        CancellationToken cancellationToken)
    {
        return
            (from _1 in _avatarCreationSetAvatarNameUseCase.Execute(
                    new AvatarCreationSetAvatarNameRequest(profile.Id, name),
                    cancellationToken)
                from _2 in new AvatarCreationAskToLoadImagesWidget(_client).Show(new WidgetContext(profile),
                    cancellationToken)
                select true)
            .InterceptFail(_ =>
                new AvatarCreationInvalidNameWidget(_client).Show(new WidgetContext(profile), cancellationToken));
    }

    private IO<bool> TryGenerateImageByPrompt(string prompt, TelegramProfileDto profile, UserDto user,
        UserQuotasDto quotas,
        CancellationToken cancellationToken)
    {
        var imagesWillBeLeftAfterOperation = (int)user.AvailableImageGenerations -
                                             profile.Settings.ImagesToGeneratePerRequest;
        return
            from _1 in quotas.CanGenerateImages switch
            {
                false => new SimpleNotificationWidget(_client).Show(
                    new SimpleNotificationWidgetContext(profile, 
                        "Похоже что вы уже генерируете изображения, подождите пожалуйста пока они будут готовы перед созданием новых"),
                    cancellationToken),
                true => imagesWillBeLeftAfterOperation switch
                {
                    < 0 => HandleInsufficientAmountOfImageGenerations(profile, user, cancellationToken),
                    _ => HandleSufficientAmountOfImageGenerations(prompt, profile, cancellationToken)
                },
            }
            select true;
    }

    private IO<Unit> HandleInsufficientAmountOfImageGenerations(TelegramProfileDto profile, UserDto user,
        CancellationToken cancellationToken)
    {
        return from productsResponse in _getProductsUseCase.Execute(
                new GetProductsRequest(profile.Id), cancellationToken)
            from _1 in new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(profile,
                    "К сожалению, у вас недостаточно генераций, но вы всегда можете их приобрести и продолжить творить!"),
                cancellationToken)
            from _2 in new ProductsListWidget(_client, _assetsOptions).Show(
                new ProductsListWidgetContext(profile, user, productsResponse.Products, false),
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleSufficientAmountOfImageGenerations(string prompt, TelegramProfileDto profile,
        CancellationToken cancellationToken)
    {
        return from validateResponse in _validateCustomPromptUseCase.Execute(
                new ValidateCustomPromptRequest(prompt), cancellationToken)
            from _1 in validateResponse.IsValid switch
            {
                true => from generateImageResponse in _processRawUserPromptUseCase.Execute(
                            new ProcessRawUserPromptRequest(profile.Id, prompt),
                            cancellationToken)
                        .InterceptFail(_ => _client.SendMessage(profile.Id,
                            "Ошибка генерации, проверьте введённый текст пожалуйста",
                            cancellationToken: cancellationToken).ToIO())
                    from _1 in new ImageGenerationStartedWidget(_client).Show(
                        new ImageGenerationStartedWidgetContext(profile,
                            Option<(Message, Page<PromptDto>)>.None), cancellationToken)
                    select Unit.Default,
                false => new CustomPromptInvalidWidget(_client).Show(new WidgetContext(profile),
                    cancellationToken)
            }
            select Unit.Default;
    }

    private IO<bool> TryApplyPromocode(string promocode, TelegramProfileDto profile,
        CancellationToken cancellationToken)
    {
        return
            from result in _validatePromocodeUseCase.Execute(
                new ValidatePromocodeRequest(profile.Id, promocode),
                cancellationToken)
            from _1 in result.IsValid switch
            {
                true =>
                    from applicationResult in _tryApplyPromocodeUseCase.Execute(
                        new TryApplyPromocodeRequest(profile.Id, promocode), cancellationToken)
                    from _1 in new PromocodeApplicationResultWidget(_client).Show(
                        new PromocodeApplicationResultWidgetContext(profile, applicationResult.IsApplied),
                        cancellationToken)
                    select Unit.Default,
                _ => IO<Unit>.Pure(Unit.Default)
            }
            select result.IsValid;
    }
}