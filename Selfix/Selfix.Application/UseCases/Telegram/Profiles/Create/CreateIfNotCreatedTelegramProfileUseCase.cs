using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Application.ServicesAbstractions.Notifications;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.TelegramProfiles.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.Entities.Users.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Application.UseCases.Telegram.Profiles.Create;

internal sealed class CreateIfNotCreatedTelegramProfileUseCase : ICreateIfNotCreatedTelegramProfileUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly INotificationsService _notificationsService;
    private readonly ILogger _logger;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;

    public CreateIfNotCreatedTelegramProfileUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        CachedUsersRepository usersRepository, ITransactionService transactionService,
        IEnvironmentService environmentService, INotificationsService notificationsService,
        ILogger logger)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _usersRepository = usersRepository;
        _transactionService = transactionService;
        _environmentService = environmentService;
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public IO<CreateIfNotCreatedTelegramProfileResponse> Execute(CreateIfNotCreatedTelegramProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profileOption in _telegramProfilesRepository.GetById(profileId, cancellationToken).Run()
            from profileWithUser in profileOption.Match(
                p => GetProfileWithUser(p, cancellationToken),
                () => CreateProfile(profileId, request.InvitedByTelegramProfileId, cancellationToken)
            )
            select new CreateIfNotCreatedTelegramProfileResponse(profileWithUser.Item1.ToDto(),
                profileWithUser.Item2.ToDto());
    }

    private OptionT<IO, Id<User, Ulid>> GetInvitedByUserId(Option<long> invitedByTelegramProfileId,
        CancellationToken cancellationToken)
    {
        return
            from id in invitedByTelegramProfileId.ToIO()
            from profile in _telegramProfilesRepository.GetById(Id<TelegramProfile, long>.FromSafe(id),
                cancellationToken)
            select profile.UserId;
    }

    private IO<(TelegramProfile, User)> GetProfileWithUser(TelegramProfile profile, CancellationToken cancellationToken)
    {
        return
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            select (profile, user);
    }

    private IO<(TelegramProfile, User)> CreateProfile(Id<TelegramProfile, long> id,
        Option<long> invitedByTelegramProfileId, CancellationToken cancellationToken)
    {
        return
            from invitedByUserId in GetInvitedByUserId(invitedByTelegramProfileId, cancellationToken)
                .Run()
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from newUser in User.New(new NewUserSpecification(invitedByUserId, currentTime)).ToIO()
            from newProfile in TelegramProfile.New(new NewTelegramProfileSpecification(id, newUser.Id)).ToIO()
            from _1 in _transactionService.Run(
                from _1 in _usersRepository.Save(newUser, cancellationToken)
                from _2 in _telegramProfilesRepository.Save(newProfile, cancellationToken)
                from tryNumber in NaturalNumber.From(1U).ToIO()
                from _3 in _notificationsService.ScheduleSendTrialPackageToUserIfHasNoPayments(newUser.Id, tryNumber, cancellationToken)
                    .IfFail(err =>
                        IO<Unit>.Lift(() =>
                        {
                            _logger.Error(err, "Error sending notification to user {UserId} about trial package", newUser.Id);
                            return Unit.Default;
                        }))
                select Unit.Default, cancellationToken)
            select (newProfile, newUser);
    }
}