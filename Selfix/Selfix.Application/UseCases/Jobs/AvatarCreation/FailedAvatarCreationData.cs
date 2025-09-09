namespace Selfix.Application.UseCases.Jobs.AvatarCreation;

public sealed record FailedAvatarCreationData(string Error) : AvatarCreationResultData;