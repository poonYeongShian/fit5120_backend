using Deploy.Models;

namespace Deploy.Interfaces;

public interface IAnimalRepository
{
    Task<Animal?> GetAnimalByIdAsync(int animalId);
    Task<AnimalClass?> GetAnimalClassByIdAsync(int classId);
    Task<ConservationStatus?> GetConservationStatusByIdAsync(int conservationStatusId);
    Task<IEnumerable<(Animal Animal, AnimalClass? AnimalClass, ConservationStatus? ConservationStatus)>> GetAllAnimalsWithDetailsAsync(string? animalClass = null);
    Task<IEnumerable<AnimalOccurrence>> GetOccurrencesByAnimalIdAsync(int animalId);
    Task<IEnumerable<(ThreatDetail Detail, ThreatCategory Category)>> GetThreatDetailsByAnimalIdAsync(int animalId);
    Task<IEnumerable<(HabitatDetail Detail, HabitatCategory Category)>> GetHabitatDetailsByAnimalIdAsync(int animalId);
}
