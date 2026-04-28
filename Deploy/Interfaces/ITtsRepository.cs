using Deploy.Models;

namespace Deploy.Interfaces;

public interface ITtsRepository
{
    Task<TtsCacheEntry?> GetByKeyAsync(string textHash, string voiceId, string modelId);
    Task<TtsCacheEntry?> GetByTextHashAsync(string textHash);
    Task SaveAsync(TtsCacheEntry entry);
}
