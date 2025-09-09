using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared;

namespace Selfix.Domain.ValueObjects.Telegram.Profiles.Settings;

public sealed class TelegramProfileSettings
{
    private TelegramProfileSettings(ImageAspectRatioEnum imageAspectRatio, NaturalNumber imagesPerRequest)
    {
        ImageAspectRatio = imageAspectRatio;
        ImagesPerRequest = imagesPerRequest;
    }

    public ImageAspectRatioEnum ImageAspectRatio { get; }
    public NaturalNumber ImagesPerRequest { get; }

    public static TelegramProfileSettings New()
    {
        return new TelegramProfileSettings(ImageAspectRatioEnum.Square1X1, NaturalNumber.From(2U).ThrowIfFail());
    }

    public static Fin<TelegramProfileSettings> From(ImageAspectRatioEnum imageAspectRatio,
        NaturalNumber imagesPerRequest)
    {
        if (imagesPerRequest > 3U) return Error.New("Images per request more than 3 is not supported");
        return new TelegramProfileSettings(imageAspectRatio, imagesPerRequest);
    }

    public Fin<TelegramProfileSettings> ChangeAspectRatio(ImageAspectRatioEnum imageAspectRatio)
    {
        return From(imageAspectRatio, ImagesPerRequest);
    }

    public Fin<TelegramProfileSettings> ChangeImagesPerRequest(NaturalNumber imagesPerRequest)
    {
        return From(ImageAspectRatio, imagesPerRequest);
    }
}