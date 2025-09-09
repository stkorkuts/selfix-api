using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Domain.ValueObjects.Promocodes;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Promocodes.TryApplyPromocode;

internal sealed class TryApplyPromocodeUseCase : ITryApplyPromocodeUseCase
{
    private readonly IProductsRepository _productsRepository;
    private readonly IPromocodesRepository _promocodesRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;

    public TryApplyPromocodeUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        IPromocodesRepository promocodesRepository, IProductsRepository productsRepository,
        CachedUsersRepository usersRepository, ITransactionService transactionService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _promocodesRepository = promocodesRepository;
        _productsRepository = productsRepository;
        _usersRepository = usersRepository;
        _transactionService = transactionService;
    }

    public IO<TryApplyPromocodeResponse> Execute(TryApplyPromocodeRequest request, CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from code in AlphanumericString.From(request.Promocode).ToIO()
            from promocode in _promocodesRepository.GetByCode(code, cancellationToken)
                .ToIOFailIfNone(Error.New($"Promocode with code: {request.Promocode} not found"))
            from isApplied in promocode.UsedByUserId.Match(
                _ => IO<bool>.Pure(false),
                () => ApplyPromocode(profile, promocode, cancellationToken).Map(_ => true))
            select new TryApplyPromocodeResponse(isApplied);
    }

    private IO<Unit> ApplyPromocode(TelegramProfile profile, Promocode promocode, CancellationToken cancellationToken)
    {
        return
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from product in _productsRepository.GetById(promocode.ProductId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Product with id: {promocode.ProductId} not found"))
            from _1 in product.Data switch
            {
                PackageProductData data => IO<Unit>.Pure(user.ApplyPackage(data)),
                _ => Error.New("There is no handler for products other than packages yet")
            }
            from _2 in promocode.Use(user.Id).ToIO()
            from _3 in _transactionService.Run(
                from _1 in _promocodesRepository.Save(promocode, cancellationToken)
                from _2 in _usersRepository.Save(user, cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }
}