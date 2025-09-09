using System;

namespace Selfix.Application.UseCases.Statistics.Application;

public sealed record GetApplicationStatisticsRequest(Ulid UserId);