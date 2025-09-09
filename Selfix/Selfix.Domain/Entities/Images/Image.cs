using LanguageExt;
using Selfix.Domain.Entities.Images.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Images;
using Selfix.Domain.ValueObjects.ObjectStorage;

namespace Selfix.Domain.Entities.Images;

public sealed class Image
{
    private Image(Id<Image, Ulid> id, OSFilePath path, DateTimeOffset createdAt, Id<User, Ulid> userId)
    {
        Id = id;
        Path = path;
        CreatedAt = createdAt;
        UserId = userId;
    }

    public Id<Image, Ulid> Id { get; }
    public Id<User, Ulid> UserId { get; }
    public OSFilePath Path { get; }
    public DateTimeOffset CreatedAt { get; }

    public static Fin<Image> New(NewImageSpecification specs)
    {
        var id = Id<Image, Ulid>.FromSafe(Ulid.NewUlid());
        return new Image(id, specs.Path, specs.CreatedAt, specs.UserId);
    }
    
    public static Fin<Image> Restore(RestoreImageSpecification specs)
    {
        return new Image(specs.Id, specs.Path, specs.CreatedAt, specs.UserId);
    }
}