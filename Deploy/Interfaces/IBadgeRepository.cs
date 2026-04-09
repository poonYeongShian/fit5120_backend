using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IBadgeRepository
{
    Task<IEnumerable<BadgeCollectionDto>> GetBadgeCollectionAsync(Guid profileId);
}
