using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IFunFactRepository
{
    Task<IEnumerable<AnimalFunFactDto>> GetFunFactsByAnimalAsync(int animalId, Guid profileId);
}
