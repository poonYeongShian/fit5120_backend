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
            SELECT m.id              AS Id,
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
            FROM   public.missions m
            JOIN   public.mission_types mt ON mt.id = m.mission_type_id
            WHERE  m.is_active = TRUE
              AND  m.is_outdoor = (
                       SELECT is_outdoor_safe
                       FROM   public.weather_conditions
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
            INSERT INTO public.profile_missions
                (profile_id, mission_id, weather_code, weather_temp, weather_is_day,
                 location_lat, location_lon, status, points_earned, assigned_at)
            VALUES
                (@ProfileId, @MissionId, @WeatherCode, @WeatherTemp, @IsDay,
                 @LocationLat, @LocationLon, 'assigned', 0, now())
            RETURNING id
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
            UPDATE public.profile_missions
            SET    status     = 'in_progress',
                   started_at = now()
            WHERE  id     = @ProfileMissionId
              AND  status = 'assigned'
            """,
            new { ProfileMissionId = profileMissionId });

        return rows > 0;
    }

    public async Task<CompleteMissionResponseDto?> CompleteMissionAsync(int profileMissionId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        // Step 1: Mark mission as completed; get points_reward and profile_id from the row
        var step1 = await connection.QueryFirstOrDefaultAsync<(int PointsEarned, Guid ProfileId)?>(
            """
            UPDATE public.profile_missions pm
            SET    status        = 'completed',
                   points_earned = m.points_reward,
                   completed_at  = now()
            FROM   public.missions m
            WHERE  pm.mission_id  = m.id
              AND  pm.id          = @ProfileMissionId
              AND  pm.status     != 'completed'
            RETURNING pm.points_earned AS PointsEarned, pm.profile_id AS ProfileId
            """,
            new { ProfileMissionId = profileMissionId },
            transaction);

        if (step1 is null)
            return null;

        var points = step1.Value.PointsEarned;
        var profileId = step1.Value.ProfileId;

        // Step 2: Add points and increment mission counter
        await connection.ExecuteAsync(
            """
            UPDATE public.profile_progress
            SET    total_points   = total_points + @Points,
                   total_missions = total_missions + 1,
                   last_active_at = now(),
                   updated_at     = now()
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId, Points = points },
            transaction);

        // Step 3: Level-up check
        var previousLevel = await connection.QuerySingleAsync<int>(
            "SELECT current_level FROM public.profile_progress WHERE profile_id = @ProfileId",
            new { ProfileId = profileId },
            transaction);

        await connection.ExecuteAsync(
            """
            UPDATE public.profile_progress
            SET    current_level = (
                       SELECT COALESCE(MAX(level_number), 1)
                       FROM   public.levels
                       WHERE  points_required <= (
                           SELECT total_points
                           FROM   public.profile_progress
                           WHERE  profile_id = @ProfileId
                       )
                   ),
                   updated_at = now()
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId },
            transaction);

        // Step 4: Unlock new fun facts based on new level
        var newFactsUnlocked = await connection.ExecuteAsync(
            """
            INSERT INTO public.profile_unlocked_facts (profile_id, fact_id)
            SELECT @ProfileId, f.id
            FROM   public.animal_fun_facts f
            WHERE  f.unlock_level <= (
                       SELECT current_level
                       FROM   public.profile_progress
                       WHERE  profile_id = @ProfileId
                   )
            ON CONFLICT (profile_id, fact_id) DO NOTHING
            """,
            new { ProfileId = profileId },
            transaction);

        // Step 5: Award level badge if leveled up
        await connection.ExecuteAsync(
            """
            INSERT INTO public.profile_badges (profile_id, badge_id, source)
            SELECT @ProfileId, b.id, 'mission'
            FROM   public.badges b
            WHERE  b.badge_type     = 'level'
              AND  b.level_required = (
                       SELECT current_level
                       FROM   public.profile_progress
                       WHERE  profile_id = @ProfileId
                   )
              AND  b.is_active = TRUE
            ON CONFLICT (profile_id, badge_id) DO NOTHING
            """,
            new { ProfileId = profileId },
            transaction);

        // Step 6: Award mission milestone badge if milestone hit
        await connection.ExecuteAsync(
            """
            INSERT INTO public.profile_badges (profile_id, badge_id, source)
            SELECT @ProfileId, b.id, 'mission'
            FROM   public.badges b
            WHERE  b.badge_type        = 'mission'
              AND  b.missions_required = (
                       SELECT total_missions
                       FROM   public.profile_progress
                       WHERE  profile_id = @ProfileId
                   )
              AND  b.is_active = TRUE
            ON CONFLICT (profile_id, badge_id) DO NOTHING
            """,
            new { ProfileId = profileId },
            transaction);

        // Read back updated progress
        var progress = await connection.QuerySingleAsync<(int TotalPoints, int CurrentLevel)>(
            """
            SELECT total_points   AS TotalPoints,
                   current_level  AS CurrentLevel
            FROM   public.profile_progress
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId },
            transaction);

        // Read back any newly awarded badges from this transaction
        var newBadges = (await connection.QueryAsync<BadgeDto>(
            """
            SELECT b.badge_name     AS BadgeName,
                   b.badge_image_url AS BadgeImageUrl,
                   b.description    AS Description
            FROM   public.profile_badges pb
            JOIN   public.badges b ON b.id = pb.badge_id
            WHERE  pb.profile_id = @ProfileId
              AND  pb.earned_at >= now() - INTERVAL '5 seconds'
            """,
            new { ProfileId = profileId },
            transaction)).ToList();

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
