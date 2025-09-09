using Selfix.Domain.Entities.Images;

namespace Selfix.Application.Dtos.Images;

internal static class ImageMappingExtensions
{
    public static ImageDto ToDto(this Image image)
    {
        return new ImageDto(image.Id, image.Path);
    }
}