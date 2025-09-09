using System;

namespace Selfix.Application.Dtos.Statistics;

public sealed record ApplicationStatisticsDto(uint CurrentlyGeneratingPromptsAndImages, uint CurrentlyCreatingAvatars, uint TodayOrdersCount);
