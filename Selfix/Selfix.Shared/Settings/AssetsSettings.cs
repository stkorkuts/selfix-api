namespace Selfix.Shared.Settings;

public sealed class AssetsSettings
{
    public const string KEY = "Assets";
    
    public required AssetsImagesSettings Images { get; init; }
}