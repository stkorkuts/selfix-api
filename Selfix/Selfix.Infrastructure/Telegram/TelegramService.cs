using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.Dtos.Images;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.Promocodes;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.List;
using Selfix.Infrastructure.Telegram.Widgets.Images.Result;
using Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;
using Selfix.Infrastructure.Telegram.Widgets.Products.List;
using Selfix.Infrastructure.Telegram.Widgets.Products.Paid;
using Selfix.Infrastructure.Telegram.Widgets.Promocodes.Paid;
using Selfix.Shared.Constants;
using Selfix.Shared.Settings;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram;

internal sealed class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _client;
    private readonly IOptions<AssetsSettings> _assetsOptions;
    private readonly IOptions<TelegramBotSettings> _telegramBotOptions;

    public TelegramService(ITelegramBotClient client, IOptions<AssetsSettings> assetsOptions,
        IOptions<TelegramBotSettings> telegramBotOptions)
    {
        _client = client;
        _assetsOptions = assetsOptions;
        _telegramBotOptions = telegramBotOptions;
    }

    public IO<Unit> PutFileIntoStream(TelegramFile telegramFile, Stream destination,
        CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var file = await _client.GetInfoAndDownloadFile(telegramFile.Identifier, destination, cancellationToken);
            if (string.IsNullOrWhiteSpace(file.FilePath))
                throw new FileNotFoundException($"Can not find telegram file with id: {telegramFile}");
            destination.Position = 0;
            return Unit.Default;
        });
    }

    public IO<Unit> ShowSimpleNotificationWidget(TelegramProfileDto profile, string message,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new SimpleNotificationWidget(_client)
            .Show(new SimpleNotificationWidgetContext(profile, message), cancellationToken));
    }

    private static IO<Unit> CatchBotBlockedByUserError(IO<Unit> io)
    {
        return io.Catch(
            err => err.Message.Contains("bot was blocked by the user"),
            err => IO<Unit>.Pure(Unit.Default));
    }

    public IO<Unit> ShowPhotoWidgets(TelegramProfileDto profile, Iterable<(ImageDto, Uri)> imagesWithSignedUris,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new ImagesGenerationResultWidget(_client, _telegramBotOptions).Show(
            new ImagesGenerationResultWidgetContext(profile, imagesWithSignedUris), cancellationToken));
    }

    public IO<Unit> ShowTrialPackagesWidget(TelegramProfileDto profile, UserDto user, Iterable<ProductDto> products,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new ProductsListWidget(_client, _assetsOptions).Show(
            new ProductsListWidgetContext(profile, user, products, false),
            cancellationToken));
    }

    public IO<Unit> ShowOrderCanceledWidget(TelegramProfileDto profile, OrderDto order,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new SimpleNotificationWidget(_client).Show(
            new SimpleNotificationWidgetContext(profile, "Заказ был отменён"),
            cancellationToken));
    }

    public IO<Unit> ShowPromocodePaidWidget(TelegramProfileDto profile, PromocodeDto promocode,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new PromocodePaidWidget(_client).Show(new PromocodePaidWidgetContext(profile, promocode),
            cancellationToken));
    }

    public IO<Unit> ShowProductPaidWidget(TelegramProfileDto profile, ProductDto product,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new ProductPaidWidget(_client).Show(new ProductPaidWidgetContext(profile, product), cancellationToken));
    }

    public IO<Unit> ShowAvatarCreationFailedWidget(TelegramProfileDto profile, CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(profile,
                "К сожалению не смогли создать для вас аватар, уже решаем проблему, попробуйте ещё раз пожалуйста"),
            cancellationToken));
    }

    public IO<Unit> ShowImageGenerationFailedWidget(TelegramProfileDto profile, CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(profile,
                "К сожалению не смогли создать для вас изображения, уже решаем проблему, попробуйте ещё раз пожалуйста"),
            cancellationToken));
    }

    public IO<Unit> ShowAvatarsListWidget(TelegramProfileDto profile, Iterable<AvatarDto> avatars,
        CancellationToken cancellationToken)
    {
        return CatchBotBlockedByUserError(new AvatarsListWidget(_client).Show(new AvatarsListWidgetContext(profile, avatars), cancellationToken));
    }

    public IO<Unit> ShowRefBonusGivenWidget(TelegramProfileDto profile, TelegramProfileDto invitedByProfile,
        CancellationToken cancellationToken)
    {
        return
            CatchBotBlockedByUserError(
                from _1 in new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(profile,
                    $"Круто, Вы пришли по приглашению и {Constants.REFERAL_BONUS_GENERATIONS} дополнительных генераций уже добавлены в ваш аккаунт!"), cancellationToken)
                from _2 in new SimpleNotificationWidget(_client).Show(new SimpleNotificationWidgetContext(invitedByProfile,
                    $"Круто, Вы пригласили друга и {Constants.REFERAL_BONUS_GENERATIONS} дополнительных генераций уже добавлены в ваш аккаунт! Так держать!"), cancellationToken)
                select Unit.Default);
    }
}