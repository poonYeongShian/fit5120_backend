using Dapper;
using Deploy.DTOs;
using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Repositories;

public class FunFactRepository : IFunFactRepository
{
    private readonly NpgsqlConnection _connection;

    public FunFactRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<AnimalFunFactDto>> GetFunFactsByAnimalAsync(int animalId, Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<AnimalFunFactDto>(
            """
            SELECT
                f.id                                                    AS Id,
                f.emoji                                                 AS Emoji,
                f.fact_text                                             AS FactText,
                f.fact_image_url                                        AS FactImageUrl,
                f.fact_order                                            AS FactOrder,
                f.unlock_level                                          AS UnlockLevel,
                f.is_locked                                             AS IsLocked,

                CASE
                    WHEN f.is_locked = TRUE                         THEN 'locked'
                    WHEN f.unlock_level > pp.current_level          THEN 'locked'
                    ELSE                                                 'unlocked'
                END                                                     AS AccessStatus,

                GREATEST(f.unlock_level - pp.current_level, 0)         AS LevelsNeeded,

                pp.current_level                                        AS UserLevel,
                pp.total_points                                         AS UserPoints,
                l.level_name                                            AS UserLevelName,
                l.badge_emoji                                           AS BadgeEmoji,

                CASE
                    WHEN puf.id IS NOT NULL THEN TRUE
                    ELSE FALSE
                END                                                     AS AlreadyUnlocked

            FROM public.animal_fun_facts f

            JOIN public.profile_progress pp
                ON pp.profile_id = @ProfileId

            JOIN public.levels l
                ON l.level_number = pp.current_level

            LEFT JOIN public.profile_unlocked_facts puf
                ON puf.profile_id = pp.profile_id
               AND puf.fact_id    = f.id

            WHERE f.animal_id = @AnimalId
            ORDER BY f.fact_order
            """,
            new { AnimalId = animalId, ProfileId = profileId });
    }
}
