using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IBadgeService
{
    Task<IEnumerable<BadgeCollectionDto>?> GetBadgeCollectionAsync(Guid profileId);
}
