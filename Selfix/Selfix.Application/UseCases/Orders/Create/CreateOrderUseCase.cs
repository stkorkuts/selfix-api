using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.Payments;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Orders.Specifications;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.Promocodes.Specifications;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders.Products;
using Selfix.Domain.ValueObjects.Orders.Promocodes;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Orders.Create;

internal sealed class CreateOrderUseCase : ICreateOrderUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ITelegramPaymentsService _telegramPaymentsService;
    private readonly IProductsRepository _productsRepository;
    private readonly IPromocodesRepository _promocodesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public CreateOrderUseCase(IEnvironmentService environmentService, IOrdersRepository ordersRepository,
        IProductsRepository productsRepository, IPromocodesRepository promocodesRepository,
        ITelegramPaymentsService telegramPaymentsService, CachedUsersRepository usersRepository,
        CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _environmentService = environmentService;
        _ordersRepository = ordersRepository;
        _productsRepository = productsRepository;
        _promocodesRepository = promocodesRepository;
        _telegramPaymentsService = telegramPaymentsService;
        _usersRepository = usersRepository;
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<CreateOrderResponse> Execute(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from product in _productsRepository
                .GetById(Id<Product, Ulid>.FromSafe(request.ProductId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Product with id: {request.ProductId} not found"))
            from order in request.IsPromocode switch
            {
                false => CreateProductOrder(user, product, cancellationToken),
                true => CreatePromocodeOrder(user, product, cancellationToken)
            }
            from paymentData in _telegramPaymentsService.CreatePayment(profile.ToDto(), order.ToDto(), product.ToDto(), cancellationToken)
            from _1 in order.SetPaymentData(paymentData).ToIO()
            from _2 in _ordersRepository.Save(order, cancellationToken)
            select new CreateOrderResponse(order.ToDto());
    }

    private IO<Order> CreateProductOrder(User user, Product product, CancellationToken cancellationToken)
    {
        return
            from _1 in IO<Unit>.Pure(Unit.Default)
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from order in Order.New(new NewOrderSpecification(user.Id, new ProductOrderData(product.Id),
                currentTime)).ToIO()
            select order;
    }

    private IO<Order> CreatePromocodeOrder(User user, Product product, CancellationToken cancellationToken)
    {
        return
            from promocode in Promocode.New(new NewPromocodeSpecification(product.Id)).ToIO()
            from _1 in _promocodesRepository.Save(promocode, cancellationToken)
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from order in Order.New(new NewOrderSpecification(user.Id, new PromocodeOrderData(promocode.Id),
                currentTime)).ToIO()
            select order;
    }
}