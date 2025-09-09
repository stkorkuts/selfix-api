using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Statistics;

public sealed class UserStatistics
{
    private UserStatistics(Id<User, Ulid> userId, uint currentlyGeneratingImagesAndPrompts, uint currentlyGeneratingAvatars)
    {
        UserId = userId;
        CurrentlyGeneratingImagesAndPrompts = currentlyGeneratingImagesAndPrompts;
        CurrentlyGeneratingAvatars = currentlyGeneratingAvatars;
    }
    
    public Id<User, Ulid> UserId { get; }
    public uint CurrentlyGeneratingImagesAndPrompts { get; }
    public uint CurrentlyGeneratingAvatars { get; }

    public static UserStatistics From(Id<User, Ulid> userId, uint currentlyGeneratedImagesAndPrompts,
        uint currentlyGeneratedAvatars)
    {
        return new UserStatistics(userId, currentlyGeneratedImagesAndPrompts, currentlyGeneratedAvatars);
    }
}