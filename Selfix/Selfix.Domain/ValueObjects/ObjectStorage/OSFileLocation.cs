namespace Selfix.Domain.ValueObjects.ObjectStorage;

public sealed class OSFileLocation
{
    private OSFileLocation(OSBucketName bucket, OSFilePath path)
    {
        Bucket = bucket;
        FilePath = path;
    }

    public OSBucketName Bucket { get; }
    public OSFilePath FilePath { get; }

    public static OSFileLocation Create(OSBucketName bucket, OSFilePath path)
    {
        return new OSFileLocation(bucket, path);
    }
}