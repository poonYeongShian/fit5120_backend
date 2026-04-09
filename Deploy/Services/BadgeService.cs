using Deploy.DTOs;
using Deploy.Interfaces;

namespace Deploy.Services;

public class BadgeService : IBadgeService
{
    private readonly IBadgeRepository _repository;
    private readonly IProfileRepository _profileRepository;

    public BadgeService(IBadgeRepository repository, IProfileRepository profileRepository)
    {
        _repository        = repository;
        _profileRepository = profileRepository;
    }

    public async Task<IEnumerable<BadgeCollectionDto>?> GetBadgeCollectionAsync(Guid profileId)
    {
        var level = await _profileRepository.GetCurrentLevelAsync(profileId);

        if (level is null)
            return null;

        return await _repository.GetBadgeCollectionAsync(profileId);
    }
}
