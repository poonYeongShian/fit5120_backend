using Dapper;
using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Models;
using Npgsql;

namespace Deploy.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly NpgsqlConnection _connection;

    public ProfileRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<Profile> CreateProfileAsync(string profileCode, string pin, string displayName, int animalId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<Profile>(
            @"INSERT INTO public.profiles
                  (profile_code, pin, display_name, animal_id)
              VALUES
                  (@ProfileCode, @Pin, @DisplayName, @AnimalId)
              RETURNING
                  id           AS Id,
                  profile_code AS ProfileCode,
                  pin          AS Pin,
                  display_name AS DisplayName,
                  animal_id    AS AnimalId,
                  avatar_url   AS AvatarUrl,
                  is_active    AS IsActive,
                  created_at   AS CreatedAt,
                  updated_at   AS UpdatedAt",
            new { ProfileCode = profileCode, Pin = pin, DisplayName = displayName, AnimalId = animalId });
    }

    public async Task<ProfileProgress> CreateProfileProgressAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<ProfileProgress>(
            @"INSERT INTO public.profile_progress
                  (profile_id, current_level, total_points)
              VALUES
                  (@ProfileId, 1, 0)
              RETURNING
                  id             AS Id,
                  profile_id     AS ProfileId,
                  current_level  AS CurrentLevel,
                  total_points   AS TotalPoints,
                  total_quizzes  AS TotalQuizzes,
                  total_correct  AS TotalCorrect,
                  total_missions AS TotalMissions,
                  streak_days    AS StreakDays,
                  last_active_at AS LastActiveAt,
                  updated_at     AS UpdatedAt",
            new { ProfileId = profileId });
    }

    public async Task<ProfileSession> CreateProfileSessionAsync(Guid profileId, string sessionToken, string? deviceInfo)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<ProfileSession>(
            @"INSERT INTO public.profile_sessions
                  (profile_id, session_token, device_info)
              VALUES
                  (@ProfileId, @SessionToken, @DeviceInfo)
              RETURNING
                  id            AS Id,
                  profile_id    AS ProfileId,
                  session_token AS SessionToken,
                  device_info   AS DeviceInfo,
                  is_active     AS IsActive,
                  created_at    AS CreatedAt,
                  expires_at    AS ExpiresAt,
                  last_used_at  AS LastUsedAt",
            new { ProfileId = profileId, SessionToken = sessionToken, DeviceInfo = deviceInfo });
    }

    public async Task<ProfileAutoLoginRow?> GetProfileBySessionTokenAsync(string sessionToken)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<ProfileAutoLoginRow>(
            @"SELECT
                  p.id              AS ProfileId,
                  p.display_name    AS DisplayName,
                  p.animal_id       AS AnimalId,
                  p.profile_code    AS ProfileCode,
                  pp.current_level  AS CurrentLevel,
                  pp.total_points   AS TotalPoints,
                  pp.total_quizzes  AS TotalQuizzes,
                  pp.total_missions AS TotalMissions,
                  pp.streak_days    AS StreakDays
              FROM public.profile_sessions ps
              JOIN public.profiles         p  ON p.id          = ps.profile_id
              JOIN public.profile_progress pp ON pp.profile_id = p.id
              WHERE ps.session_token = @SessionToken
                AND ps.is_active     = TRUE
                AND (ps.expires_at IS NULL OR ps.expires_at > now())",
            new { SessionToken = sessionToken });
    }

    public async Task TouchSessionLastUsedAsync(string sessionToken)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE public.profile_sessions
              SET last_used_at = now()
              WHERE session_token = @SessionToken",
            new { SessionToken = sessionToken });
    }

    public async Task<bool> InvalidateSessionAsync(string sessionToken)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        var rows = await connection.ExecuteAsync(
            @"UPDATE public.profile_sessions
              SET    is_active = FALSE
              WHERE  session_token = @SessionToken
                AND  is_active     = TRUE",
            new { SessionToken = sessionToken });

        // Returns true only when a live session was actually found and deactivated
        return rows > 0;
    }

    public async Task<RestoreProfileRow?> GetProfileByCodeAndPinAsync(string profileCode, string pin)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<RestoreProfileRow>(
            @"SELECT
                  p.id             AS ProfileId,
                  p.display_name   AS DisplayName,
                  p.animal_id      AS AnimalId,
                  p.profile_code   AS ProfileCode,
                  pp.current_level AS CurrentLevel,
                  pp.total_points  AS TotalPoints,
                  pp.streak_days   AS StreakDays
              FROM public.profiles         p
              JOIN public.profile_progress pp ON pp.profile_id = p.id
              WHERE p.profile_code = @ProfileCode
                AND p.pin          = @Pin
                AND p.is_active    = TRUE",
            new { ProfileCode = profileCode, Pin = pin });
    }

    public async Task<int?> GetCurrentLevelAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<int?>(
            @"SELECT current_level
              FROM   public.profile_progress
              WHERE  profile_id = @ProfileId",
            new { ProfileId = profileId });
    }

    public async Task<ProfileProgress?> GetProfileProgressAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<ProfileProgress>(
            @"SELECT id            AS Id,
                     profile_id    AS ProfileId,
                     current_level AS CurrentLevel,
                     total_points  AS TotalPoints,
                     total_quizzes AS TotalQuizzes,
                     total_correct AS TotalCorrect,
                     total_missions AS TotalMissions,
                     streak_days   AS StreakDays,
                     last_active_at AS LastActiveAt,
                     updated_at    AS UpdatedAt
              FROM   public.profile_progress
              WHERE  profile_id = @ProfileId",
            new { ProfileId = profileId });
    }

    public async Task AddPointsAndIncrementMissionsAsync(
        Guid profileId, int points, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
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
    }

    public async Task<int> GetCurrentLevelAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return await connection.QuerySingleAsync<int>(
            "SELECT current_level FROM public.profile_progress WHERE profile_id = @ProfileId",
            new { ProfileId = profileId },
            transaction);
    }

    public async Task UpdateLevelAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
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
    }

    public async Task<int> UnlockNewFunFactsAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return await connection.ExecuteAsync(
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
    }

    public async Task AwardLevelBadgeAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
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
    }

    public async Task AwardMissionMilestoneBadgeAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
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
    }

    public async Task<(int TotalPoints, int CurrentLevel)> GetProgressAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return await connection.QuerySingleAsync<(int TotalPoints, int CurrentLevel)>(
            """
            SELECT total_points   AS TotalPoints,
                   current_level  AS CurrentLevel
            FROM   public.profile_progress
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId },
            transaction);
    }

    public async Task<List<BadgeDto>> GetRecentBadgesAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return (await connection.QueryAsync<BadgeDto>(
            """
            SELECT b.badge_name      AS BadgeName,
                   b.badge_image_url AS BadgeImageUrl,
                   b.description     AS Description
            FROM   public.profile_badges pb
            JOIN   public.badges b ON b.id = pb.badge_id
            WHERE  pb.profile_id = @ProfileId
              AND  pb.earned_at >= now() - INTERVAL '5 seconds'
            """,
            new { ProfileId = profileId },
            transaction)).ToList();
    }

    public async Task<int> InsertQuizHistoryAsync(
        Guid profileId, int score, int totalQuestions,
        int correctAnswers, int pointsEarned, int levelBefore,
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        return await connection.QuerySingleAsync<int>(
            """
            INSERT INTO public.profile_history
                (profile_id, score, total_questions,
                 correct_answers, points_earned, level_before)
            VALUES
                (@ProfileId, @Score, @TotalQuestions,
                 @CorrectAnswers, @PointsEarned, @LevelBefore)
            RETURNING id
            """,
            new
            {
                ProfileId = profileId,
                Score = score,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                PointsEarned = pointsEarned,
                LevelBefore = levelBefore
            },
            transaction);
    }

    public async Task AddPointsAndIncrementQuizzesAsync(
        Guid profileId, int points, int correctAnswers,
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await connection.ExecuteAsync(
            """
            UPDATE public.profile_progress
            SET    total_points   = total_points + @Points,
                   total_quizzes  = total_quizzes + 1,
                   total_correct  = total_correct + @CorrectAnswers,
                   last_active_at = now(),
                   updated_at     = now()
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId, Points = points, CorrectAnswers = correctAnswers },
            transaction);
    }

    public async Task UpdateQuizHistoryLevelAfterAsync(
        int historyId, Guid profileId,
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await connection.ExecuteAsync(
            """
            UPDATE public.profile_history
            SET    level_after = (
                       SELECT current_level
                       FROM   public.profile_progress
                       WHERE  profile_id = @ProfileId
                   ),
                   leveled_up = (
                       level_before < (
                           SELECT current_level
                           FROM   public.profile_progress
                           WHERE  profile_id = @ProfileId
                       )
                   )
            WHERE  id = @HistoryId
            """,
            new { HistoryId = historyId, ProfileId = profileId },
            transaction);
    }

    public async Task AwardQuizLevelBadgeAsync(
        Guid profileId, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO public.profile_badges (profile_id, badge_id, source)
            SELECT @ProfileId, b.id, 'quiz'
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
    }
}
