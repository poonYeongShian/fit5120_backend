using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Mappers;

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
        var progress = await _profileRepository.GetProfileProgressAsync(profileId);

        if (progress is null)
            return null;

        var facts       = await _repository.GetFunFactsByAnimalIdAsync(animalId);
        var unlockedIds = new HashSet<int>(await _repository.GetUnlockedFactIdsByProfileAsync(profileId));

        return FunFactMapper.ToAnimalFunFactDtoList(facts, progress, unlockedIds);
    }

    public async Task<IEnumerable<AnimalFunFactDto>?> GetAllFunFactsAsync(Guid profileId)
    {
        var progress = await _profileRepository.GetProfileProgressAsync(profileId);

        if (progress is null)
            return null;

        var facts       = await _repository.GetAllFunFactsAsync();
        var unlockedIds = new HashSet<int>(await _repository.GetUnlockedFactIdsByProfileAsync(profileId));

        return FunFactMapper.ToAnimalFunFactDtoList(facts, progress, unlockedIds);
    }
}
