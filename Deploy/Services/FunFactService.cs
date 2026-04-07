using Deploy.DTOs;
using Deploy.Interfaces;

namespace Deploy.Services;

public class FunFactService : IFunFactService
{
    private readonly IFunFactRepository _repository;
    private readonly IProfileRepository _profileRepository;

    public FunFactService(IFunFactRepository repository, IProfileRepository profileRepository)
    {
        _repository        = repository;
        _profileRepository = profileRepository;
    }

    public async Task<IEnumerable<AnimalFunFactDto>?> GetFunFactsByAnimalAsync(int animalId, Guid profileId)
    {
        // Guard: profile must exist before we run the joined query
        var level = await _profileRepository.GetCurrentLevelAsync(profileId);

        if (level is null)
            return null;

        return await _repository.GetFunFactsByAnimalAsync(animalId, profileId);
    }
}
