using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Users.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Products.Packages;

namespace Selfix.Domain.Entities.Users;

public sealed class User
{
    private User(Id<User, Ulid> id, Option<Id<User, Ulid>> invitedByUserId, uint availableAvatarGenerations,
        uint availableImageGenerations, bool hasPayments, Option<Id<Avatar, Ulid>> activeAvatarId, DateTimeOffset createdAt,
        bool isAdmin)
    {
        Id = id;
        InvitedByUserId = invitedByUserId;
        AvailableAvatarGenerations = availableAvatarGenerations;
        AvailableImageGenerations = availableImageGenerations;
        HasPayments = hasPayments;
        ActiveAvatarId = activeAvatarId;
        CreatedAt = createdAt;
        IsAdmin = isAdmin;
    }

    public Id<User, Ulid> Id { get; private set; }
    public Option<Id<User, Ulid>> InvitedByUserId { get; private set; }
    public uint AvailableAvatarGenerations { get; private set; }
    public uint AvailableImageGenerations { get; private set; }
    public bool HasPayments { get; private set; }
    public Option<Id<Avatar, Ulid>> ActiveAvatarId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsAdmin { get; private set; }

    public static Fin<User> New(NewUserSpecification specs)
    {
        return new User(Id<User, Ulid>.FromSafe(Ulid.NewUlid()), specs.InvitedById, 0U, 0U, false,
            Option<Id<Avatar, Ulid>>.None, specs.CreatedAt, false);
    }
    
    public static Fin<User> Restore(RestoreUserSpecification specs)
    {
        return new User(specs.Id, specs.InvitedByUserId, specs.AvailableAvatarGenerations,
            specs.AvailableImageGenerations, specs.HasPayments, specs.ActiveAvatarId, specs.CreatedAt, specs.IsAdmin);
    }

    public Fin<Unit> RequestAvatarCreation()
    {
        if (AvailableAvatarGenerations < 1)
            return Error.New("No available avatar generations");

        AvailableAvatarGenerations--;
        return Unit.Default;
    }

    public Fin<Unit> SetActiveAvatarId(Avatar avatar)
    {
        if (!avatar.UserId.Equals(Id)) return Error.New("Avatar of another user can not be set as active one");

        ActiveAvatarId = avatar.Id;
        return Unit.Default;
    }

    public Fin<Unit> RequestImagesGeneration(NaturalNumber count)
    {
        if (AvailableImageGenerations < count)
            return Error.New("There is not enough available image generations");

        AvailableImageGenerations -= count;
        return Unit.Default;
    }

    public Unit ApplyPackage(PackageProductData data)
    {
        AvailableAvatarGenerations += data.AvatarGenerationsCount;
        AvailableImageGenerations += data.ImageGenerationsCount;
        return Unit.Default;
    }

    public Unit AddAvatarGenerations(NaturalNumber count)
    {
        AvailableAvatarGenerations += count;
        return Unit.Default;
    }

    public Unit AddImageGenerations(NaturalNumber count)
    {
        AvailableImageGenerations += count;
        return Unit.Default;
    }
}