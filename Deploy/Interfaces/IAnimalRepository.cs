using Deploy.Models;

namespace Deploy.Interfaces;

public interface IAnimalRepository
{
    Task<Animal?> GetAnimalByIdAsync(int animalId);
    Task<Category?> GetCategoryByIdAsync(int categoryId);
    Task<ConservationStatus?> GetConservationStatusByIdAsync(int conservationStatusId);
    Task<IEnumerable<(Animal Animal, Category? Category, ConservationStatus? ConservationStatus)>> GetAllAnimalsWithDetailsAsync();
    Task<IEnumerable<AnimalOccurrence>> GetOccurrencesByAnimalIdAsync(int animalId);
}
