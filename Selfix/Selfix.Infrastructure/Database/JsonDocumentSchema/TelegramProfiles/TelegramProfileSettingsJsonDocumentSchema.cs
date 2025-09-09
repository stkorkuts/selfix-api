using Selfix.Shared;

namespace Selfix.Infrastructure.Database.JsonDocumentSchema.TelegramProfile;

public record TelegramProfileSettingsJsonDocumentSchema(ImageAspectRatioEnum AspectRatio, uint ImagesPerRequest);