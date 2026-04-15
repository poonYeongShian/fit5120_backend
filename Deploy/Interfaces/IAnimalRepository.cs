using Deploy.Models;

namespace Deploy.Interfaces;

public interface IAnimalRepository
{
    Task<Animal?> GetAnimalByIdAsync(int animalId);
    Task<AnimalGroup?> GetAnimalGroupByIdAsync(int groupId);
    Task<ConservationStatus?> GetConservationStatusByIdAsync(int conservationStatusId);
    Task<IEnumerable<(Animal Animal, AnimalGroup? AnimalGroup, ConservationStatus? ConservationStatus)>> GetAllAnimalsWithDetailsAsync(string? animalGroup = null);
    Task<IEnumerable<AnimalOccurrence>> GetOccurrencesByAnimalIdAsync(int animalId);
    Task<IEnumerable<(ThreatDetail Detail, ThreatCategory Category)>> GetThreatDetailsByAnimalIdAsync(int animalId);
    Task<IEnumerable<(HabitatDetail Detail, HabitatCategory Category)>> GetHabitatDetailsByAnimalIdAsync(int animalId);
}
