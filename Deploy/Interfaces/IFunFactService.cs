using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IFunFactService
{
    Task<IEnumerable<AnimalFunFactDto>?> GetFunFactsByAnimalAsync(int animalId, Guid profileId);
    Task<IEnumerable<AnimalFunFactDto>?> GetAllFunFactsAsync(Guid profileId);
}
