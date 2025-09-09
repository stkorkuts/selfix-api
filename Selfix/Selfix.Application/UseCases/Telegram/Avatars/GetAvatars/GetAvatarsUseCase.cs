using LanguageExt;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.UseCases.Telegram.Avatars.GetAvatars;

internal sealed class GetAvatarsUseCase : IGetAvatarsUseCase
{
    private readonly IAvatarsRepository _avatarsRepository;

    public GetAvatarsUseCase(IAvatarsRepository avatarsRepository)
    {
        _avatarsRepository = avatarsRepository;
    }

    public IO<GetAvatarsResponse> Execute(GetAvatarsRequest request, CancellationToken cancellationToken)
    {
        var id = Id<User, Ulid>.FromSafe(request.UserId);
        return
            from avatars in _avatarsRepository.GetUserAvatars(id, cancellationToken)
            let avatarDtos = avatars.Map(a => a.ToDto())
            select new GetAvatarsResponse(avatarDtos);
    }
}