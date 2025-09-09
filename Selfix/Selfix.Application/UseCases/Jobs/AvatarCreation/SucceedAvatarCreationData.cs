namespace Selfix.Application.UseCases.Jobs.AvatarCreation;

public sealed record SucceedAvatarCreationData(string Description, string LoraPath)
    : AvatarCreationResultData;