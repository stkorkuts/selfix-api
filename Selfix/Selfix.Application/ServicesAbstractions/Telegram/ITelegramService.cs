using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.Dtos.Images;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.Promocodes;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Telegram.Files;

namespace Selfix.Application.ServicesAbstractions.Telegram;

public interface ITelegramService
{
    IO<Unit> PutFileIntoStream(TelegramFile telegramFile, Stream destination, CancellationToken cancellationToken);
    IO<Unit> ShowSimpleNotificationWidget(TelegramProfileDto profile, string message, CancellationToken cancellationToken);
    IO<Unit> ShowPhotoWidgets(TelegramProfileDto profile, Iterable<(ImageDto, Uri)> imagesWithSignedUris, CancellationToken cancellationToken);

    IO<Unit> ShowTrialPackagesWidget(TelegramProfileDto profile, UserDto user, Iterable<ProductDto> products,
        CancellationToken cancellationToken);

    IO<Unit> ShowOrderCanceledWidget(TelegramProfileDto profile, OrderDto order, CancellationToken cancellationToken);
    IO<Unit> ShowPromocodePaidWidget(TelegramProfileDto profile, PromocodeDto promocode, CancellationToken cancellationToken);
    IO<Unit> ShowProductPaidWidget(TelegramProfileDto profile, ProductDto product, CancellationToken cancellationToken);
    IO<Unit> ShowAvatarCreationFailedWidget(TelegramProfileDto profile, CancellationToken cancellationToken);
    IO<Unit> ShowImageGenerationFailedWidget(TelegramProfileDto profile, CancellationToken cancellationToken);
    IO<Unit> ShowAvatarsListWidget(TelegramProfileDto profile, Iterable<AvatarDto> avatars, CancellationToken cancellationToken);
    IO<Unit> ShowRefBonusGivenWidget(TelegramProfileDto profile, TelegramProfileDto invitedByProfile, CancellationToken cancellationToken);
}