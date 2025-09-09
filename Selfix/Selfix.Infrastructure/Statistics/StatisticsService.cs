using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Statistics;
using Selfix.Infrastructure.Database;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Statistics;

internal sealed class StatisticsService : IStatisticsService
{
    private readonly SelfixDbContext _dbContext;

    public StatisticsService(SelfixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IO<UserStatistics> GetUserStatistics(Id<User, Ulid> userId, CancellationToken cancellationToken)
    {
        return GetStatistics(
            query => query.Where(j => j.UserId == userId),
            async query =>
            {
                var imageAndPromptCount = await query
                    .CountAsync(j => j.Type == JobTypeEnum.ImageGeneration || j.Type == JobTypeEnum.PromptProcessing, cancellationToken);
                var avatarCount = await query
                    .CountAsync(j => j.Type == JobTypeEnum.AvatarCreation, cancellationToken);

                return UserStatistics.From(userId, (uint)imageAndPromptCount, (uint)avatarCount);
            });
    }

    public IO<ApplicationStatistics> GetApplicationStatistics(CancellationToken cancellationToken)
    {
        return GetStatistics(
            query => query,
            async query =>
            {
                var imageAndPromptCount = await query
                    .CountAsync(j => j.Type == JobTypeEnum.ImageGeneration || j.Type == JobTypeEnum.PromptProcessing, cancellationToken);
                var avatarCount = await query
                    .CountAsync(j => j.Type == JobTypeEnum.AvatarCreation, cancellationToken);

                var today = DateTimeOffset.UtcNow.Date;
                var todayOrdersCount = await _dbContext.Orders
                    .CountAsync(o => o.CreatedAt.Date == today, cancellationToken);

                return ApplicationStatistics.From((uint)imageAndPromptCount, (uint)avatarCount, (uint)todayOrdersCount);
            });
    }

    private IO<T> GetStatistics<T>(
        Func<IQueryable<JobDb>, IQueryable<JobDb>> filter,
        Func<IQueryable<JobDb>, Task<T>> mapToStatistics)
    {
        return IO<T>.LiftAsync(async () =>
        {
            var query = filter(_dbContext.Jobs
                .Where(j => j.Status == JobStatusEnum.Processing));

            return await mapToStatistics(query);
        });
    }
}