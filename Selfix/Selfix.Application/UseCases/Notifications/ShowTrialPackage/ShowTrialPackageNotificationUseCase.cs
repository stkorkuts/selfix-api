using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Notifications;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Application.UseCases.Notifications.ShowTrialPackage;

internal sealed class ShowTrialPackageNotificationUseCase : IShowTrialPackageNotificationUseCase
{
    private readonly IProductsRepository _productsRepository;
    private readonly INotificationsService _notificationsService;
    private readonly ILogger _logger;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly IUsersRepository _usersRepository;

    public ShowTrialPackageNotificationUseCase(ITelegramService telegramService,
        CachedTelegramProfilesRepository telegramProfilesRepository,
        CachedUsersRepository usersRepository, IProductsRepository productsRepository,
        INotificationsService notificationsService, ILogger logger)
    {
        _telegramService = telegramService;
        _telegramProfilesRepository = telegramProfilesRepository;
        _usersRepository = usersRepository;
        _productsRepository = productsRepository;
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public IO<ShowTrialPackageNotificationResponse> Execute(ShowTrialPackageNotificationRequest request,
        CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository
                .GetByUserId(Id<User, Ulid>.FromSafe(request.UserId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {request.UserId} not found"))
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from _1 in user.HasPayments
                ? IO<Unit>.Pure(Unit.Default)
                : ShowTrialPackages(profile, user, request.CurrentTryNumber + 1, cancellationToken)
            select new ShowTrialPackageNotificationResponse();
    }

    private IO<Unit> ShowTrialPackages(TelegramProfile profile, User user, uint nextTryNumber,
        CancellationToken cancellationToken)
    {
        return
            from trialProducts in _productsRepository.GetByType(ProductTypeEnum.TrialPackage, cancellationToken)
            from _1 in _telegramService.ShowTrialPackagesWidget(profile.ToDto(), user.ToDto(),
                trialProducts.Map(p => p.ToDto()), cancellationToken)
            from tryNumber in NaturalNumber.From(nextTryNumber).ToIO()
            from _2 in _notificationsService.ScheduleSendTrialPackageToUserIfHasNoPayments(user.Id, tryNumber, cancellationToken)
                .IfFail(err =>
                    IO<Unit>.Lift(() =>
                    {
                        _logger.Error(err, "Error sending notification to user {UserId} about trial package", user.Id);
                        return Unit.Default;
                    }))
            select Unit.Default;
    }
}