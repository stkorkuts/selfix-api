using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Statistics;

namespace Selfix.Domain.ValueObjects.Quotas;

public sealed class UserQuotas
{
    private readonly User _user;
    private readonly UserStatistics _statistics;

    private const uint AVATAR_GENERATIONS_THRESHOLD = 1;
    private const uint IMAGE_GENERATIONS_THRESHOLD = 3;

    private UserQuotas(User user, UserStatistics statistics)
    {
        _user = user;
        _statistics = statistics;
    }
    
    public static Fin<UserQuotas> From(User user, UserStatistics statistics)
    {
        if(!user.Id.Equals(statistics.UserId)) return Error.New("User id does not match statistics user id");
        return new UserQuotas(user, statistics);
    }

    public bool CanGenerateAvatars()
    {
        if (_user.IsAdmin) return true;
        return _statistics.CurrentlyGeneratingAvatars < AVATAR_GENERATIONS_THRESHOLD;
    }
    
    public bool CanGenerateImages()
    {
        if (_user.IsAdmin) return true;
        return _statistics.CurrentlyGeneratingImagesAndPrompts < IMAGE_GENERATIONS_THRESHOLD;
    }
}