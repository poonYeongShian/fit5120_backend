using Dapper;
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
                  p.id            AS ProfileId,
                  p.display_name  AS DisplayName,
                  p.animal_id     AS AnimalId,
                  p.profile_code  AS ProfileCode,
                  pp.current_level AS CurrentLevel,
                  pp.total_points  AS TotalPoints,
                  pp.total_quizzes AS TotalQuizzes,
                  pp.streak_days   AS StreakDays,
                  l.level_name     AS LevelName,
                  l.badge_emoji    AS BadgeEmoji
              FROM public.profile_sessions ps
              JOIN public.profiles         p  ON p.id           = ps.profile_id
              JOIN public.profile_progress pp ON pp.profile_id  = p.id
              JOIN public.levels           l  ON l.level_number = pp.current_level
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
                  pp.streak_days   AS StreakDays,
                  l.level_name     AS LevelName,
                  l.badge_emoji    AS BadgeEmoji
              FROM public.profiles         p
              JOIN public.profile_progress pp ON pp.profile_id  = p.id
              JOIN public.levels           l  ON l.level_number = pp.current_level
              WHERE p.profile_code = @ProfileCode
                AND p.pin          = @Pin
                AND p.is_active    = TRUE",
            new { ProfileCode = profileCode, Pin = pin });
    }

    // ?? Save Progress ???????????????????????????????????????????????????????????

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

    public async Task<int> InsertQuizHistoryAsync(
        Guid profileId,
        int score, int totalQuestions, int correctAnswers,
        int pointsEarned, int levelBefore)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<int>(
            @"INSERT INTO public.profile_history
                  (profile_id,
                   score, total_questions, correct_answers,
                   points_earned, level_before)
              VALUES
                  (@ProfileId,
                   @Score, @TotalQuestions, @CorrectAnswers,
                   @PointsEarned, @LevelBefore)
              RETURNING id",
            new
            {
                ProfileId      = profileId,
                Score          = score,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                PointsEarned   = pointsEarned,
                LevelBefore    = levelBefore
            });
    }

    public async Task AddPointsAsync(Guid profileId, int pointsEarned, int correctAnswers)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            @"UPDATE public.profile_progress
              SET
                  total_points   = total_points  + @PointsEarned,
                  total_quizzes  = total_quizzes + 1,
                  total_correct  = total_correct + @CorrectAnswers,
                  last_active_at = now(),
                  updated_at     = now()
              WHERE profile_id = @ProfileId",
            new { ProfileId = profileId, PointsEarned = pointsEarned, CorrectAnswers = correctAnswers });
    }

    public async Task<ProgressAfterQuizRow> ApplyLevelUpAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        // Step 1: Resolve and apply the highest level the profile has earned.
        await connection.ExecuteAsync(
            @"UPDATE public.profile_progress
              SET
                  current_level = (
                      SELECT MAX(l.level_number)
                      FROM   public.levels l
                      WHERE  l.points_required <= (
                          SELECT total_points
                          FROM   public.profile_progress
                          WHERE  profile_id = @ProfileId
                      )
                  ),
                  updated_at = now()
              WHERE profile_id = @ProfileId",
            new { ProfileId = profileId });

        // Step 2: Read back the resolved level together with its display metadata.
        return await connection.QuerySingleAsync<ProgressAfterQuizRow>(
            @"SELECT
                  pp.current_level AS CurrentLevel,
                  pp.total_points  AS TotalPoints,
                  l.level_name     AS LevelName,
                  l.badge_emoji    AS BadgeEmoji
              FROM public.profile_progress pp
              JOIN public.levels           l  ON l.level_number = pp.current_level
              WHERE pp.profile_id = @ProfileId",
            new { ProfileId = profileId });
    }

    public async Task<int> UnlockNewFactsAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        // Inserts one row per fact whose unlock_level <= the profile's current level.
        // ON CONFLICT DO NOTHING skips facts already unlocked.
        // Returns the count of newly inserted rows.
        return await connection.ExecuteAsync(
            @"INSERT INTO public.profile_unlocked_facts (profile_id, fact_id)
              SELECT @ProfileId, f.id
              FROM   public.animal_fun_facts f
              WHERE  f.unlock_level <= (
                  SELECT current_level
                  FROM   public.profile_progress
                  WHERE  profile_id = @ProfileId
              )
              ON CONFLICT (profile_id, fact_id) DO NOTHING",
            new { ProfileId = profileId });
    }

    public async Task FinaliseHistoryAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        // Stamps level_after and leveled_up on the most-recent history row for this profile.
        await connection.ExecuteAsync(
            @"UPDATE public.profile_history
              SET
                  level_after = (
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
              WHERE id = (
                  SELECT id
                  FROM   public.profile_history
                  WHERE  profile_id = @ProfileId
                  ORDER  BY completed_at DESC
                  LIMIT  1
              )",
            new { ProfileId = profileId });
    }
}
