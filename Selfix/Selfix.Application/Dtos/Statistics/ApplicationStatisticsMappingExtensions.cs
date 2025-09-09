using System;
using Selfix.Domain.ValueObjects.Statistics;

namespace Selfix.Application.Dtos.Statistics;

internal static class ApplicationStatisticsMappingExtensions
{
    public static ApplicationStatisticsDto ToDto(this ApplicationStatistics statistics)
    {
        return new ApplicationStatisticsDto(statistics.CurrentlyGeneratingPromptsAndImages, statistics.CurrentlyCreatingAvatars, statistics.TodayOrdersCount);
    }
}
