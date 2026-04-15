using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Mappers;

namespace Deploy.Services;

public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;

    public AnimalService(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AnimalCardDto>> GetAllAnimalCardsAsync(string? animalGroup = null)
    {
        var items = await _repository.GetAllAnimalsWithDetailsAsync(animalGroup);
        return AnimalMapper.ToAnimalCardDtoList(items);
    }

    public async Task<AnimalCardDetailDto?> GetAnimalCardDetailAsync(int animalId)
    {
        var animal = await _repository.GetAnimalByIdAsync(animalId);

        if (animal is null)
            return null;

        var animalGroup = await _repository.GetAnimalGroupByIdAsync(animal.GroupId);
        var conservationStatus = await _repository.GetConservationStatusByIdAsync(animal.ConservationStatusId);
        var threats = await _repository.GetThreatDetailsByAnimalIdAsync(animalId);
        var habitats = await _repository.GetHabitatDetailsByAnimalIdAsync(animalId);

        return AnimalMapper.ToAnimalCardDetailDto(animal, animalGroup, conservationStatus, threats, habitats);
    }

    public async Task<IEnumerable<AnimalOccurrenceDto>?> GetAnimalOccurrencesAsync(int animalId)
    {
        var animal = await _repository.GetAnimalByIdAsync(animalId);

        if (animal is null)
            return null;

        var occurrences = await _repository.GetOccurrencesByAnimalIdAsync(animalId);
        return AnimalMapper.ToAnimalOccurrenceDtoList(occurrences);
    }
}
