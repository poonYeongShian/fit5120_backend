using Deploy.Models;

namespace Deploy.Interfaces;

public interface IProfileRepository
{
    Task<Profile> CreateProfileAsync(string profileCode, string pin, string displayName, int animalId);
    Task<ProfileProgress> CreateProfileProgressAsync(Guid profileId);
    Task<ProfileSession> CreateProfileSessionAsync(Guid profileId, string sessionToken, string? deviceInfo);

    Task<ProfileAutoLoginRow?> GetProfileBySessionTokenAsync(string sessionToken);
    Task TouchSessionLastUsedAsync(string sessionToken);
    Task<bool> InvalidateSessionAsync(string sessionToken);

    Task<RestoreProfileRow?> GetProfileByCodeAndPinAsync(string profileCode, string pin);
    Task<int?> GetCurrentLevelAsync(Guid profileId);
}
