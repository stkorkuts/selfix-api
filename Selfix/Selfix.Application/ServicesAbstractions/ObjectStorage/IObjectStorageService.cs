using LanguageExt;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Shared;

namespace Selfix.Application.ServicesAbstractions.ObjectStorage;

public interface IObjectStorageService
{
    IO<Unit> UploadFile(Stream fileStream, OSFileLocation location, CancellationToken cancellationToken);
    IO<Unit> DeleteFileIfExist(OSFileLocation location, CancellationToken cancellationToken);
    IO<Uri> GetSignedUriForReading(OSFileLocation location, CancellationToken cancellationToken);
    IO<Uri> GetSignedUriForWriting(OSFileLocation location, CancellationToken cancellationToken);

    IO<OSBucketName> GetBucketName(BucketContentTypeEnum contentType);
}