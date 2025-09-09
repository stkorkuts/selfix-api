using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Avatars.Specifications;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Infrastructure.Database.Entities;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class AvatarsRepository : IAvatarsRepository
{
    private readonly SelfixDbContext _context;

    public AvatarsRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public IO<Iterable<Avatar>> GetUserAvatars(Id<User, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Iterable<Avatar>>.LiftAsync(async () =>
        {
            var avatars = await _context.Avatars.Where(a => a.UserId == id).ToListAsync(cancellationToken);
            return avatars.AsIterable().Traverse(FromDb).As().ThrowIfFail();
        });
    }

    public OptionT<IO, Avatar> GetActiveByUserId(Id<User, Ulid> userId, CancellationToken cancellationToken)
    {
        return IO<Option<Avatar>>.LiftAsync(async () =>
        {
            var avatar =
                await _context.Avatars.FirstOrDefaultAsync(a => a.ActiveForUser!.Id == userId, cancellationToken);
            return avatar is null ? Option<Avatar>.None : FromDb(avatar).ThrowIfFail();
        });
    }

    public OptionT<IO, Avatar> GetById(Id<Avatar, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<Avatar>>.LiftAsync(async () =>
        {
            var avatar = await _context.Avatars.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
            return avatar is null ? Option<Avatar>.None : FromDb(avatar).ThrowIfFail();
        });
    }

    public IO<Unit> Save(Avatar avatar, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var existingAvatar = await _context.Avatars.AsTracking()
                .FirstOrDefaultAsync(a => a.Id == avatar.Id, cancellationToken);
            if (existingAvatar is not null)
            {
                existingAvatar.Name = avatar.Name;
                existingAvatar.Description = avatar.Description;
                existingAvatar.OSLoraFilePath = avatar.OSFilePath;
            }
            else
            {
                var avatarDb = new AvatarDb
                {
                    Id = avatar.Id,
                    Name = avatar.Name,
                    Description = avatar.Description,
                    OSLoraFilePath = avatar.OSFilePath,
                    UserId = avatar.UserId,
                    CreatedAt = avatar.CreatedAt
                };
                _context.Add(avatarDb);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private static Fin<Avatar> FromDb(AvatarDb entity)
    {
        var id = Id<Avatar, Ulid>.FromSafe(entity.Id);
        var userId = Id<User, Ulid>.FromSafe(entity.UserId);
        return
            from name in AvatarName.From(entity.Name)
            from description in AvatarDescription.From(entity.Description)
            from loraFilePath in OSFilePath.From(entity.OSLoraFilePath)
            from avatar in Avatar.Restore(new RestoreAvatarSpecification(id, userId, name, description, loraFilePath,
                entity.CreatedAt))
            select avatar;
    }
}