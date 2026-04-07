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

    // ?? Save Progress (5 steps) ?????????????????????????????????????????????????
    Task<int?> GetCurrentLevelAsync(Guid profileId);
    Task<int> InsertQuizHistoryAsync(Guid profileId,
                                     int score, int totalQuestions, int correctAnswers,
                                     int pointsEarned, int levelBefore);
    Task AddPointsAsync(Guid profileId, int pointsEarned, int correctAnswers);
    Task<ProgressAfterQuizRow> ApplyLevelUpAsync(Guid profileId);
    Task<int> UnlockNewFactsAsync(Guid profileId);
    Task FinaliseHistoryAsync(Guid profileId);
}
