using System;

namespace Selfix.Domain.ValueObjects.Statistics;

public class ApplicationStatistics
{
    private ApplicationStatistics(uint currentlyGeneratingPromptsAndImages, uint currentlyCreatingAvatars, uint todayOrdersCount)
    {
        CurrentlyGeneratingPromptsAndImages = currentlyGeneratingPromptsAndImages;
        CurrentlyCreatingAvatars = currentlyCreatingAvatars;
        TodayOrdersCount = todayOrdersCount;
    }

    public uint CurrentlyGeneratingPromptsAndImages { get; }
    public uint CurrentlyCreatingAvatars { get; }
    public uint TodayOrdersCount { get; }

    public static ApplicationStatistics From(uint currentlyGeneratingPromptsAndImages, uint currentlyCreatingAvatars, uint todayOrdersCount)
    {
        return new ApplicationStatistics(currentlyGeneratingPromptsAndImages, currentlyCreatingAvatars, todayOrdersCount);
    }
}
