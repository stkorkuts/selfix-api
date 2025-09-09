using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Images;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Result;

internal sealed record ImagesGenerationResultWidgetContext(
    TelegramProfileDto Profile,
    Iterable<(ImageDto Image, Uri SignedUri)> ImagesWithSignedUris)
    : WidgetContext(Profile);