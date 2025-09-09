using Selfix.Infrastructure.Database.Extensions;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class UserDb : IDbEntity<Ulid>
{
    public required Ulid Id { get; set; }
    public required int AvatarGenerationsCount { get; set; }
    public required int ImageGenerationsCount { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public IList<AvatarDb>? Avatars { get; set; }
    public IList<ImageDb>? Images { get; set; }
    public IList<JobDb>? Jobs { get; set; }
    public IList<OrderDb>? Orders { get; set; }
    public IList<UserDb>? InvitedUsers { get; set; }
    public IList<PromocodeDb>? UsedPromocodes { get; set; }

    public Ulid? InvitedById { get; set; }
    public UserDb? InvitedBy { get; set; }

    public TelegramProfileDb? TelegramProfile { get; set; }
    
    public Ulid? ActiveAvatarId { get; set; }
    public AvatarDb? ActiveAvatar { get; set; }
}