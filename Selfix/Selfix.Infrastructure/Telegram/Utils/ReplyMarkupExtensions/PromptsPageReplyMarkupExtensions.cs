using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;

internal static class PromptsPageReplyMarkupExtensions
{
    public static Fin<InlineKeyboardMarkup> GetReplyMarkup(this Page<PromptDto> page)
    {
        var totalPages = (uint)(
            page.Total / TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE +
            (page.Total % TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE == 0 ? 0 : 1));
        var currentPageIndex = page.Skipped / TelegramConstants.PUBLIC_PROMPTS_PAGE_SIZE;

        return totalPages switch
        {
            1 => from promptButtons in GetPromptButtons(page.Items, currentPageIndex)
                select new InlineKeyboardMarkup(promptButtons),
            _ => from promptButtons in GetPromptButtons(page.Items, currentPageIndex)
                from navButtons in GetNavigationButtons(totalPages, currentPageIndex)
                select new InlineKeyboardMarkup(promptButtons.Add(navButtons))
        };
    }

    private static Fin<Iterable<InlineKeyboardButton[]>> GetPromptButtons(Iterable<PromptDto> prompts,
        uint currentPageIndex)
    {
        var promptsWithCallbacks =
            (from p in prompts
                    .Traverse(p =>
                        CallbackSerializer.Serialize(new PredefinedPromptsGenerateImageCallbackData
                            { PromptId = p.Id, PageIndex = currentPageIndex }))
                select p.Zip(prompts))
            .As();

        return
            from p in promptsWithCallbacks
            let b = p
                .Map(promptWithCallback =>
                    InlineKeyboardButton.WithCallbackData(promptWithCallback.Second.Name, promptWithCallback.First))
                .Chunk(2)
                .AsIterable()
            select b;
    }

    private static Fin<InlineKeyboardButton[]> GetNavigationButtons(uint totalPages, uint currentPageIndex)
    {
        return
            from leftButton in GetLeftPageButton(totalPages, currentPageIndex)
            from rightButton in GetRightPageButton(totalPages, currentPageIndex)
            select new[] { leftButton, rightButton };
    }

    private static Fin<InlineKeyboardButton> GetLeftPageButton(uint totalPages, uint currentPageIndex)
    {
        var leftPageIndex = currentPageIndex == 0 ? totalPages - 1 : currentPageIndex - 1;

        return
            from callbackData in CallbackSerializer.Serialize(new PredefinedPromptsSwitchPageCallbackData
                { TargetPageIndex = leftPageIndex })
            select InlineKeyboardButton.WithCallbackData("\u2b05", callbackData);
    }

    private static Fin<InlineKeyboardButton> GetRightPageButton(uint totalPages, uint currentPageIndex)
    {
        var rightPageIndex = currentPageIndex == totalPages - 1 ? 0 : currentPageIndex + 1;

        return
            from callbackData in CallbackSerializer.Serialize(new PredefinedPromptsSwitchPageCallbackData
                { TargetPageIndex = rightPageIndex })
            select InlineKeyboardButton.WithCallbackData("\u27a1", callbackData);
    }
}