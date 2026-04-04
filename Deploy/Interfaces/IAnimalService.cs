using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IAnimalService
{
    Task<IEnumerable<AnimalCardDto>> GetAllAnimalCardsAsync();
    Task<AnimalCardDetailDto?> GetAnimalCardDetailAsync(int animalId);
    Task<IEnumerable<AnimalOccurrenceDto>?> GetAnimalOccurrencesAsync(int animalId);
}
