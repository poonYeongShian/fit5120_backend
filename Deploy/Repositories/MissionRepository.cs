using Dapper;
using Deploy.DTOs;
using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Repositories;

public class MissionRepository : IMissionRepository
{
    private readonly NpgsqlConnection _connection;

    public MissionRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<WeatherMissionDto?> GetWeatherAdaptiveMissionAsync(int weatherCode, bool isDay)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<WeatherMissionDto>(
            """
            SELECT m.mission_id      AS Id,
                   m.title           AS Title,
                   m.description     AS Description,
                   m.step_1          AS Step1,
                   m.step_2          AS Step2,
                   m.step_3          AS Step3,
                   m.time_limit_min  AS TimeLimitMin,
                   m.is_outdoor      AS IsOutdoor,
                   m.is_day_only     AS IsDayOnly,
                   m.points_reward   AS PointsReward,
                   m.image_url       AS ImageUrl,
                   mt.type_name      AS TypeName
            FROM   public.mission m
            JOIN   public.mission_type mt ON mt.mission_type_id = m.mission_type_id
            WHERE  m.is_active = TRUE
              AND  m.is_outdoor = (
                       SELECT is_outdoor_safe
                       FROM   public.weather_condition
                       WHERE  @WeatherCode BETWEEN wmo_code_min AND wmo_code_max
                       LIMIT  1
                   )
              AND  (m.is_day_only = FALSE OR @IsDay = TRUE)
            ORDER BY RANDOM()
            LIMIT 1
            """,
            new { WeatherCode = weatherCode, IsDay = isDay });
    }

    public async Task<int?> AssignMissionAsync(
        Guid profileId, int missionId, int? weatherCode, bool? isDay,
        decimal? weatherTemp, decimal? locationLat, decimal? locationLon)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        var profileMissionId = await connection.QueryFirstOrDefaultAsync<int?>(
            """
            INSERT INTO public.profile_mission
                (profile_id, mission_id, weather_code, weather_temp, weather_is_day,
                 location_lat, location_lon, status, points_earned, assigned_at)
            VALUES
                (@ProfileId, @MissionId, @WeatherCode, @WeatherTemp, @IsDay,
                 @LocationLat, @LocationLon, 'assigned', 0, now())
            RETURNING profile_mission_id
            """,
            new
            {
                ProfileId = profileId,
                MissionId = missionId,
                WeatherCode = weatherCode,
                WeatherTemp = weatherTemp,
                IsDay = isDay,
                LocationLat = locationLat,
                LocationLon = locationLon
            });

        return profileMissionId;
    }

    public async Task<bool> StartMissionAsync(int profileMissionId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        var rows = await connection.ExecuteAsync(
            """
            UPDATE public.profile_mission
            SET    status     = 'in_progress',
                   started_at = now()
            WHERE  profile_mission_id = @ProfileMissionId
              AND  status = 'assigned'
            """,
            new { ProfileMissionId = profileMissionId });

        return rows > 0;
    }

    public async Task<(int PointsEarned, Guid ProfileId)?> MarkMissionCompletedAsync(
        int profileMissionId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return await connection.QueryFirstOrDefaultAsync<(int PointsEarned, Guid ProfileId)?>(
            """
            UPDATE public.profile_mission pm
            SET    status        = 'completed',
                   points_earned = m.points_reward,
                   completed_at  = now()
            FROM   public.mission m
            WHERE  pm.mission_id          = m.mission_id
              AND  pm.profile_mission_id  = @ProfileMissionId
              AND  pm.status             != 'completed'
            RETURNING pm.points_earned AS PointsEarned, pm.profile_id AS ProfileId
            """,
            new { ProfileMissionId = profileMissionId },
            transaction);
    }

    public async Task<List<CompletedMissionHistoryDto>> GetCompletedMissionHistoryAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        var results = await connection.QueryAsync<CompletedMissionHistoryDto>(
            """
            SELECT m.mission_id      AS MissionId,
                   m.title           AS Title,
                   m.description     AS Description,
                   mt.type_name      AS TypeName,
                   pm.points_earned  AS PointsEarned,
                   m.is_outdoor      AS IsOutdoor,
                   m.image_url       AS ImageUrl,
                   pm.completed_at   AS CompletedAt
            FROM   public.profile_mission pm
            JOIN   public.mission m       ON m.mission_id       = pm.mission_id
            JOIN   public.mission_type mt ON mt.mission_type_id = m.mission_type_id
            WHERE  pm.profile_id = @ProfileId
              AND  pm.status     = 'completed'
            ORDER BY pm.completed_at DESC
            """,
            new { ProfileId = profileId });

        return results.AsList();
    }
}
