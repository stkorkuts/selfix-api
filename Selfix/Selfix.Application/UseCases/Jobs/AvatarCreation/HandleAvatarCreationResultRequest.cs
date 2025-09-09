namespace Selfix.Application.UseCases.Jobs.AvatarCreation;

public sealed record HandleAvatarCreationResultRequest(Ulid JobId, AvatarCreationResultData CreationResultData);