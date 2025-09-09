using Selfix.Schema.Kafka;
using Selfix.Shared;

namespace Selfix.Infrastructure.EventStreaming;

public static class KafkaSchemaExtensions
{
    public static ImageAspectRatio ToKafkaSchema(this ImageAspectRatioEnum aspectRatioEnum)
    {
        return aspectRatioEnum switch
        {
            ImageAspectRatioEnum.Square1X1 => ImageAspectRatio.Square1X1,
            ImageAspectRatioEnum.Portrait9X16 => ImageAspectRatio.Portrait9X16,
            ImageAspectRatioEnum.Landscape16X9 => ImageAspectRatio.Landscape16X9,
            _ => throw new ArgumentOutOfRangeException(nameof(aspectRatioEnum), aspectRatioEnum, null)
        };
    }
}