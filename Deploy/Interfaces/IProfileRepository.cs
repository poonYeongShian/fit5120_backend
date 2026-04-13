using Deploy.DTOs;
using Deploy.Models;
using Npgsql;

namespace Deploy.Interfaces;

public interface IProfileRepository
{
    Task<Profile> CreateProfileAsync(string profileCode, string pin, string displayName);
    Task<ProfileProgress> CreateProfileProgressAsync(Guid profileId);
    Task<ProfileSession> CreateProfileSessionAsync(Guid profileId, string sessionToken, string? deviceInfo);

    Task<ProfileAutoLoginRow?> GetProfileBySessionTokenAsync(string sessionToken);
    Task TouchSessionLastUsedAsync(string sessionToken);
    Task<bool> InvalidateSessionAsync(string sessionToken);

    Task<RestoreProfileRow?> GetProfileByCodeAndPinAsync(string profileCode, string pin);
    Task<int?> GetCurrentLevelAsync(Guid profileId);
    Task<ProfileProgress?> GetProfileProgressAsync(Guid profileId);

    // Transactional profile-progress and reward methods (connection/transaction managed by caller)
    Task AddPointsAndIncrementMissionsAsync(Guid profileId, int points, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task<int> GetCurrentLevelAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task UpdateLevelAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task<int> UnlockNewFunFactsAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task AwardLevelBadgeAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task AwardMissionMilestoneBadgeAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task<(int TotalPoints, int CurrentLevel)> GetProgressAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task<List<BadgeDto>> GetRecentBadgesAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);

    // Transactional quiz-progress methods (connection/transaction managed by caller)
    Task<int> InsertQuizHistoryAsync(Guid profileId, int score, int totalQuestions, int correctAnswers, int pointsEarned, int levelBefore, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task AddPointsAndIncrementQuizzesAsync(Guid profileId, int points, int correctAnswers, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task UpdateQuizHistoryLevelAfterAsync(int historyId, Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
    Task AwardQuizLevelBadgeAsync(Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction);
}
