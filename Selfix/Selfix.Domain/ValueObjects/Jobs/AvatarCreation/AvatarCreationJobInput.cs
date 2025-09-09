using LanguageExt;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.ValueObjects.Jobs.AvatarCreation;

public sealed record AvatarCreationJobInput(Iterable<OSFilePath> ImagePaths, AvatarName Name);