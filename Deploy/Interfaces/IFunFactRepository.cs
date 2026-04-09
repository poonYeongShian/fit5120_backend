using Deploy.Models;

namespace Deploy.Interfaces;

public interface IFunFactRepository
{
    Task<IEnumerable<AnimalFunFact>> GetFunFactsByAnimalIdAsync(int animalId);
    Task<IEnumerable<AnimalFunFact>> GetAllFunFactsAsync();
    Task<IEnumerable<int>> GetUnlockedFactIdsByProfileAsync(Guid profileId);
}
