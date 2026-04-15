using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IAnimalService
{
    Task<IEnumerable<AnimalCardDto>> GetAllAnimalCardsAsync(string? animalGroup = null);
    Task<AnimalCardDetailDto?> GetAnimalCardDetailAsync(int animalId);
    Task<IEnumerable<AnimalOccurrenceDto>?> GetAnimalOccurrencesAsync(int animalId);
}
