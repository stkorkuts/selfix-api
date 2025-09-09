using System;
using Selfix.Application.Dtos.Statistics;

namespace Selfix.Application.UseCases.Statistics.Application;

public sealed record GetApplicationStatisticsResponse(ApplicationStatisticsDto Statistics);