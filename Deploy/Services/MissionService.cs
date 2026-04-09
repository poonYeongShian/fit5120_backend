using Deploy.DTOs;
using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly NpgsqlConnection _connection;

    public MissionService(IMissionRepository missionRepository, IProfileRepository profileRepository, NpgsqlConnection connection)
    {
        _missionRepository = missionRepository;
        _profileRepository = profileRepository;
        _connection = connection;
    }

    public async Task<WeatherMissionDto?> GetWeatherAdaptiveMissionAsync(int weatherCode, bool isDay)
    {
        return await _missionRepository.GetWeatherAdaptiveMissionAsync(weatherCode, isDay);
    }

    public async Task<int?> AssignMissionAsync(
        Guid profileId, int missionId, int? weatherCode, bool? isDay,
        decimal? weatherTemp, decimal? locationLat, decimal? locationLon)
    {
        return await _missionRepository.AssignMissionAsync(profileId, missionId, weatherCode, isDay, weatherTemp, locationLat, locationLon);
    }

    public async Task<bool> StartMissionAsync(int profileMissionId)
    {
        return await _missionRepository.StartMissionAsync(profileMissionId);
    }

    public async Task<List<CompletedMissionHistoryDto>> GetCompletedMissionHistoryAsync(Guid profileId)
    {
        return await _missionRepository.GetCompletedMissionHistoryAsync(profileId);
    }

    public async Task<CompleteMissionResponseDto?> CompleteMissionAsync(int profileMissionId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        // Step 1: Mark mission as completed; get points_reward and profile_id
        var step1 = await _missionRepository.MarkMissionCompletedAsync(profileMissionId, connection, transaction);

        if (step1 is null)
            return null;

        var points = step1.Value.PointsEarned;
        var profileId = step1.Value.ProfileId;

        // Step 2: Add points and increment mission counter
        await _profileRepository.AddPointsAndIncrementMissionsAsync(profileId, points, connection, transaction);

        // Step 3: Level-up check
        var previousLevel = await _profileRepository.GetCurrentLevelAsync(profileId, connection, transaction);
        await _profileRepository.UpdateLevelAsync(profileId, connection, transaction);

        // Step 4: Unlock new fun facts based on new level
        var newFactsUnlocked = await _profileRepository.UnlockNewFunFactsAsync(profileId, connection, transaction);

        // Step 5: Award level badge if leveled up
        await _profileRepository.AwardLevelBadgeAsync(profileId, connection, transaction);

        // Step 6: Award mission milestone badge if milestone hit
        await _profileRepository.AwardMissionMilestoneBadgeAsync(profileId, connection, transaction);

        // Read back updated progress
        var progress = await _profileRepository.GetProgressAsync(profileId, connection, transaction);

        // Read back any newly awarded badges from this transaction
        var newBadges = await _profileRepository.GetRecentBadgesAsync(profileId, connection, transaction);

        await transaction.CommitAsync();

        return new CompleteMissionResponseDto
        {
            TotalPoints = progress.TotalPoints,
            NewLevel = progress.CurrentLevel,
            LeveledUp = progress.CurrentLevel > previousLevel,
            NewFactsUnlocked = newFactsUnlocked,
            NewBadges = newBadges
        };
    }
}
