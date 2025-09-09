using LanguageExt;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Orders;

namespace Selfix.Application.ServicesAbstractions.Payments;

public interface ITelegramPaymentsService
{
    IO<OrderPaymentData> CreatePayment(TelegramProfileDto profile, OrderDto order, ProductDto product, CancellationToken cancellationToken);
}