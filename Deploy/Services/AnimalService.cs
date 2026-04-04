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

    public async Task<IEnumerable<AnimalCardDto>> GetAllAnimalCardsAsync(string? category = null)
    {
        var items = await _repository.GetAllAnimalsWithDetailsAsync(category);
        return AnimalMapper.ToAnimalCardDtoList(items);
    }

    public async Task<AnimalCardDetailDto?> GetAnimalCardDetailAsync(int animalId)
    {
        var animal = await _repository.GetAnimalByIdAsync(animalId);

        if (animal is null)
            return null;

        var category = await _repository.GetCategoryByIdAsync(animal.CategoryId);
        var conservationStatus = await _repository.GetConservationStatusByIdAsync(animal.ConservationStatusId);

        return AnimalMapper.ToAnimalCardDetailDto(animal, category, conservationStatus);
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
