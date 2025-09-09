using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.Promocodes;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Domain.ValueObjects.Orders.Products;
using Selfix.Domain.ValueObjects.Orders.Promocodes;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Shared;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Orders.Confirm;

internal sealed class ConfirmOrderUseCase : IConfirmOrderUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IPromocodesRepository _promocodesRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;
    private readonly IAvatarsRepository _avatarsRepository;

    public ConfirmOrderUseCase(IOrdersRepository ordersRepository, IProductsRepository productsRepository,
        IPromocodesRepository promocodesRepository, IEnvironmentService environmentService,
        ITransactionService transactionService, ITelegramService telegramService,
        CachedTelegramProfilesRepository telegramProfilesRepository, CachedUsersRepository usersRepository,
        IAvatarsRepository avatarsRepository)
    {
        _ordersRepository = ordersRepository;
        _productsRepository = productsRepository;
        _promocodesRepository = promocodesRepository;
        _environmentService = environmentService;
        _transactionService = transactionService;
        _telegramService = telegramService;
        _telegramProfilesRepository = telegramProfilesRepository;
        _usersRepository = usersRepository;
        _avatarsRepository = avatarsRepository;
    }

    public IO<ConfirmOrderResponse> Execute(ConfirmOrderRequest request, CancellationToken cancellationToken)
    {
        return
            from order in _ordersRepository.GetById(Id<Order, Ulid>.FromSafe(request.OrderId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Order with id: {request.OrderId} not found"))
            from user in _usersRepository.GetById(order.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {order.UserId} not found"))
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from _1 in order.ChangeStatus(new OrderStatus(OrderStatusEnum.Confirmed, currentTime)).ToIO()
            from _2 in order.Data switch
            {
                ProductOrderData productData => ConfirmProductOrder(order, user, productData, cancellationToken),
                PromocodeOrderData promocodeData => ConfirmPromocodeOrder(order, promocodeData, cancellationToken),
                _ => Error.New("There is no handler for orders other than products and promocodes yet")
            }
            // TODO: Add logging here but ignore if error happens
            from _3 in GiveRefUserBonusIfFirstPayment(user, cancellationToken).IfFail(_ => IO<Unit>.Pure(Unit.Default))
            select new ConfirmOrderResponse();
    }

    private IO<Unit> ConfirmProductOrder(Order order, User user, ProductOrderData data,
        CancellationToken cancellationToken)
    {
        return
            from product in _productsRepository.GetById(data.ProductId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Product with id: {data.ProductId} not found"))
            from _1 in product.Data switch
            {
                PackageProductData packageData => ConfirmPackageOrder(order, user, product, packageData,
                    cancellationToken),
                _ => Error.New("Only package products are supported for now")
            }
            select Unit.Default;
    }

    private IO<Unit> ConfirmPackageOrder(Order order, User user, Product product, PackageProductData packageData,
        CancellationToken cancellationToken)
    {
        user.ApplyPackage(packageData);
        return
            from _1 in _transactionService.Run(
                from _1 in _ordersRepository.Save(order, cancellationToken)
                from _2 in _usersRepository.Save(user, cancellationToken)
                from _3 in ShowTelegramProductPaidWidget(order, product, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> ShowTelegramProductPaidWidget(Order order, Product product, CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository.GetByUserId(order.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {order.UserId} not found"))
            from _1 in _telegramService.ShowProductPaidWidget(profile.ToDto(), product.ToDto(), cancellationToken)
            from _2 in product.Data switch
            {
                PackageProductData packageData => packageData.AvatarGenerationsCount switch
                {
                    > 0 =>
                        from avatars in _avatarsRepository.GetUserAvatars(profile.UserId, cancellationToken)
                        from _1 in _telegramService.ShowAvatarsListWidget(profile.ToDto(), avatars.Map(a => a.ToDto()),
                            cancellationToken)
                        select Unit.Default,
                    _ => IO<Unit>.Pure(Unit.Default)
                },
                _ => IO<Unit>.Pure(Unit.Default)
            }
            select Unit.Default;
    }

    private IO<Unit> ConfirmPromocodeOrder(Order order, PromocodeOrderData data, CancellationToken cancellationToken)
    {
        return
            from promocode in _promocodesRepository.GetById(data.PromocodeId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Promocode with id: {data.PromocodeId} not found"))
            from _2 in _transactionService.Run(
                from _1 in _ordersRepository.Save(order, cancellationToken)
                from _2 in ShowTelegramPromocodePaidWidget(order, promocode, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> ShowTelegramPromocodePaidWidget(Order order, Promocode promocode,
        CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository.GetByUserId(order.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {order.UserId} not found"))
            from _1 in _telegramService.ShowPromocodePaidWidget(profile.ToDto(), promocode.ToDto(), cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> GiveRefUserBonusIfFirstPayment(User user, CancellationToken cancellationToken)
    {
        return user.HasPayments switch
        {
            true => IO<Unit>.Pure(Unit.Default),
            false =>
                from invitedByUser in user.InvitedByUserId.Match(
                    id => _usersRepository.GetById(id, cancellationToken),
                    () => Option<User>.None).Run().As()
                from _1 in invitedByUser.Match(
                    u => GiveRefUserBonus(user, u, cancellationToken),
                    () => IO<Unit>.Pure(Unit.Default))
                select Unit.Default
        };
    }

    private IO<Unit> GiveRefUserBonus(User user, User invitedByUser, CancellationToken cancellationToken)
    {
        return
            from bonusGenerations in NaturalNumber.From(Constants.REFERAL_BONUS_GENERATIONS).ToIO()
            let _1 = user.AddImageGenerations(bonusGenerations)
            let _2 = invitedByUser.AddImageGenerations(bonusGenerations)
            from _3 in _transactionService.Run(
                from _1 in _usersRepository.Save(user, cancellationToken)
                from _2 in _usersRepository.Save(invitedByUser, cancellationToken)
                from _3 in ShowTelegramRefBonusWidget(user, invitedByUser, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }
    
    private IO<Unit> ShowTelegramRefBonusWidget(User user, User invitedByUser, CancellationToken cancellationToken)
    {
        return 
            from profile in _telegramProfilesRepository.GetByUserId(user.Id, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {user.Id} not found"))
            from invitedByProfile in _telegramProfilesRepository.GetByUserId(invitedByUser.Id, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {user.Id} not found"))
            from _1 in _telegramService.ShowRefBonusGivenWidget(profile.ToDto(), invitedByProfile.ToDto(), cancellationToken)
            select Unit.Default;
    }
}