using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Products;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Products.Get;

internal sealed class GetProductsUseCase : IGetProductsUseCase
{
    private readonly IProductsRepository _productsRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly IUsersRepository _usersRepository;

    public GetProductsUseCase(IProductsRepository productsRepository,
        CachedTelegramProfilesRepository telegramProfilesRepository,
        CachedUsersRepository usersRepository)
    {
        _productsRepository = productsRepository;
        _telegramProfilesRepository = telegramProfilesRepository;
        _usersRepository = usersRepository;
    }

    public IO<GetProductsResponse> Execute(GetProductsRequest request, CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from products in user.HasPayments
                ? _productsRepository.GetByType(ProductTypeEnum.Package, cancellationToken)
                : _productsRepository.GetByType(ProductTypeEnum.FirstPaymentPackage, cancellationToken)
            select new GetProductsResponse(products.Map(p => p.ToDto()));
    }
}