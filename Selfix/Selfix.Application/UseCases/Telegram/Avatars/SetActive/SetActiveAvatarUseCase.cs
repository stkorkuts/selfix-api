using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Avatars.SetActive;

internal sealed class SetActiveAvatarUseCase : ISetActiveAvatarUseCase
{
    private readonly IAvatarsRepository _avatarsRepository;
    private readonly IUsersRepository _usersRepository;

    public SetActiveAvatarUseCase(IAvatarsRepository avatarsRepository, CachedUsersRepository usersRepository)
    {
        _avatarsRepository = avatarsRepository;
        _usersRepository = usersRepository;
    }

    public IO<SetActiveAvatarResponse> Execute(SetActiveAvatarRequest request, CancellationToken cancellationToken)
    {
        var userId = Id<User, Ulid>.FromSafe(request.UserId);
        return
            from user in _usersRepository.GetById(userId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {userId} not found"))
            let avatarIdDomain = Id<Avatar, Ulid>.FromSafe(request.AvatarId)
            from avatar in _avatarsRepository.GetById(avatarIdDomain, cancellationToken)
                .ToIOFailIfNone(Error.New($"Avatar with id: {request.AvatarId} not found"))
            from _1 in user.SetActiveAvatarId(avatar).ToIO()
            from _2 in _usersRepository.Save(user, cancellationToken)
            select new SetActiveAvatarResponse(avatar.ToDto());
    }
}