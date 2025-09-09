namespace Selfix.Infrastructure.Database.JsonDocumentSchema.Jobs;

internal sealed record AvatarCreationJobInputJsonDocumentSchema(
    string[] ImagePaths,
    string AvatarName
    );