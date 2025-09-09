namespace Selfix.Shared.Settings;

public sealed class ObjectStorageSettings
{
    public const string KEY = "ObjectStorage";
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string Region { get; init; }
    public required string Endpoint { get; init; }
    public required string ImagesBucketName { get; init; }
    public required string AvatarsBucketName { get; init; }
    public required string TemporaryBucketName { get; init; }
}