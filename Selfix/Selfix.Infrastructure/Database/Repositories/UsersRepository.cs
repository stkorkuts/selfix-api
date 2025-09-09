using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.Entities.Users.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Infrastructure.Database.Extensions;
using Selfix.Shared;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class UsersRepository : IUsersRepository
{
    private readonly SelfixDbContext _context;
    private readonly AdministrationSettings _administrationSettings;

    public UsersRepository(SelfixDbContext context, IOptions<AdministrationSettings> administrationOptions)
    {
        _context = context;
        _administrationSettings = administrationOptions.Value;
    }

    public OptionT<IO, User> GetById(Id<User, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<User>>.LiftAsync(async () =>
        {
            var result = await _context.Users.Select(u => new
                {
                    User = u, 
                    ConfirmedOrdersCount = u.Orders!.Count(o => o.Status == OrderStatusEnum.Confirmed),
                    u.ActiveAvatar
                })
                .FirstOrDefaultAsync(u => u.User.Id == id, cancellationToken);
            
            return result is null
                ? Option<User>.None
                : FromDb(result.User, result.ConfirmedOrdersCount > 0, result.ActiveAvatar?.Id).ThrowIfFail();
        });
    }

    public IO<Unit> Save(User user, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var existingUser = await _context.Users.AsTracking().FirstOrDefaultAsync(p => p.Id == user.Id, cancellationToken);
            var activeAvatarId = user.ActiveAvatarId.Map(v => (Ulid)v).Match<Ulid?>(val => val, () => null);
            if (existingUser is not null)
            {
                existingUser.AvatarGenerationsCount = (int)user.AvailableAvatarGenerations;
                existingUser.ImageGenerationsCount = (int)user.AvailableImageGenerations;
                existingUser.ActiveAvatarId = activeAvatarId;
            }
            else
            {
                var newUser = new UserDb
                {
                    Id = user.Id,
                    AvatarGenerationsCount = (int)user.AvailableAvatarGenerations,
                    ImageGenerationsCount = (int)user.AvailableImageGenerations,
                    CreatedAt = user.CreatedAt,
                    InvitedById = user.InvitedByUserId.Map(v => (Ulid)v).Match<Ulid?>(val => val, () => null),
                    ActiveAvatarId = activeAvatarId,
                };
                _context.Add(newUser);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private Fin<User> FromDb(UserDb userDb, bool hasPayments, Ulid? activeAvatarId)
    {
        var id = Id<User, Ulid>.FromSafe(userDb.Id);
        var invitedById = Prelude.Optional(userDb.InvitedById).Map(Id<User, Ulid>.FromSafe);
        var activeAvatarIdOption = Prelude.Optional(activeAvatarId).Map(Id<Avatar, Ulid>.FromSafe);
        var isAdmin = _administrationSettings.AdminUsersIds?.Select(Ulid.Parse).Contains(userDb.Id) ?? false;
        return
            from user in User.Restore(new RestoreUserSpecification(id, invitedById, (uint)userDb.AvatarGenerationsCount,
                (uint)userDb.ImageGenerationsCount, hasPayments, activeAvatarIdOption, userDb.CreatedAt, isAdmin))
            select user;
    }
}