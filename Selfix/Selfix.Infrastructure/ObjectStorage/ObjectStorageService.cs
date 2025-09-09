using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Domain;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Shared;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.ObjectStorage;

internal sealed class ObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _client;
    private readonly ObjectStorageSettings _settings;

    public ObjectStorageService(IAmazonS3 client, IOptions<ObjectStorageSettings> settings)
    {
        _client = client;
        _settings = settings.Value;
    }

    public IO<Unit> UploadFile(Stream fileStream, OSFileLocation location, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(() => _client.UploadObjectFromStreamAsync(location.Bucket, location.FilePath,
            fileStream, new Dictionary<string, object>(), cancellationToken).ToUnit());
    }

    public IO<Unit> DeleteFileIfExist(OSFileLocation location, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
            {
                var response = await _client.DeleteObjectAsync(location.Bucket, location.FilePath, cancellationToken);
                return response.HttpStatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK ? Unit.Default : throw new Exception("Failed to delete file");
            });
    }

    public IO<Uri> GetSignedUriForReading(OSFileLocation location, CancellationToken cancellationToken)
    {
        return GetSignedUri(location, HttpVerb.GET, cancellationToken);
    }
    
    public IO<Uri> GetSignedUriForWriting(OSFileLocation location, CancellationToken cancellationToken)
    {
        return GetSignedUri(location, HttpVerb.PUT, cancellationToken);
    }

    private IO<Uri> GetSignedUri(OSFileLocation location, HttpVerb Verb, CancellationToken cancellationToken)
    {
        return IO<Uri>.LiftAsync(async () =>
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = location.Bucket,
                Key = location.FilePath,
                Verb = Verb,
                Expires = DateTime.UtcNow.AddHours(Constants.SIGNED_URI_EXPIRATION_HOURS)
            };
        
            var signedUrl = await _client.GetPreSignedURLAsync(request);
            return new Uri(signedUrl);
        });
    }

    public IO<OSBucketName> GetBucketName(BucketContentTypeEnum contentType)
    {
        return (contentType switch
        {
            BucketContentTypeEnum.Avatars => OSBucketName.From(_settings.AvatarsBucketName),
            BucketContentTypeEnum.Images => OSBucketName.From(_settings.ImagesBucketName),
            BucketContentTypeEnum.Temporary => OSBucketName.From(_settings.TemporaryBucketName),
            _ => Error.New("Unsupported bucket content type")
        }).ToIO();
    }
}