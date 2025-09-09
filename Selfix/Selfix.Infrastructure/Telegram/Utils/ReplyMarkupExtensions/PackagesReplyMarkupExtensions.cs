using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Products;
using Selfix.Infrastructure.Telegram.Callbacks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;

internal static class PackagesReplyMarkupExtensions
{
    public static Fin<InlineKeyboardMarkup> GenerateMarkup(this Iterable<ProductDto> packages, bool isPromocode)
    {
        var packagesWithCallbacks =
            (from p in packages.Traverse(p =>
                    CallbackSerializer.Serialize(new BuyPackageCallbackData
                        { PackageId = p.Id, IsPromocode = isPromocode }))
                select p.Zip(packages))
            .As();

        return
            from pc in packagesWithCallbacks
            select new InlineKeyboardMarkup(pc.Map(packageWithCallback =>
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(packageWithCallback.Second.Name, packageWithCallback.First)
                }));
    }
}