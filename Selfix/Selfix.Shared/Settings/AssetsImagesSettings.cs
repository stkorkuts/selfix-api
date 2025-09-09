namespace Selfix.Shared.Settings;

public sealed class AssetsImagesSettings
{
    public const string KEY = "Images";

    public required string PreviewImageUrl { get; init; }
    public required string ConvertImageUrl { get; init; }
    public required string PromptImageUrl { get; init; }
    public required string TutorialImageUrl { get; init; }
    public required string Price1ImageUrl { get; init; }
    public required string Price2ImageUrl { get; init; }
}