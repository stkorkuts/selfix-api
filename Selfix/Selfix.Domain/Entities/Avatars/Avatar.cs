using LanguageExt;
using Selfix.Domain.Entities.Avatars.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.Entities.Avatars;

public sealed class Avatar
{
    private Avatar(Id<Avatar, Ulid> id, Id<User, Ulid> userId, AvatarName name, AvatarDescription description,
        OSFilePath filePath, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Name = name;
        Description = description;
        OSFilePath = filePath;
        CreatedAt = createdAt;
    }

    public Id<Avatar, Ulid> Id { get; private set; }
    public Id<User, Ulid> UserId { get; private set; }
    public AvatarName Name { get; private set; }
    public AvatarDescription Description { get; private set; }
    public OSFilePath OSFilePath { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Fin<Avatar> New(NewAvatarSpecification specs)
    {
        var id = Id<Avatar, Ulid>.FromSafe(Ulid.NewUlid());
        return new Avatar(id, specs.UserId, specs.Name, specs.Description, specs.FilePath, specs.CreatedAt);
    }
    
    public static Fin<Avatar> Restore(RestoreAvatarSpecification specs)
    {
        return new Avatar(specs.Id, specs.UserId, specs.Name, specs.Description, specs.FilePath, specs.CreatedAt);
    }
}