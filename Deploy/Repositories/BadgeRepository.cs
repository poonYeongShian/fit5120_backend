using Dapper;
using Deploy.DTOs;
using Deploy.Interfaces;
using Npgsql;

namespace Deploy.Repositories;

public class BadgeRepository : IBadgeRepository
{
    private readonly NpgsqlConnection _connection;

    public BadgeRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<BadgeCollectionDto>> GetBadgeCollectionAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<BadgeCollectionDto>(
            """
            SELECT
                b.id                                                    AS Id,
                b.badge_name                                            AS BadgeName,
                b.badge_image_url                                       AS BadgeImageUrl,
                b.description                                           AS Description,
                b.badge_type::text                                      AS BadgeType,
                b.level_required                                        AS LevelRequired,
                b.missions_required                                     AS MissionsRequired,
                pb.source::text                                         AS Source,
                pb.earned_at                                            AS EarnedAt,

                CASE WHEN pb.id IS NOT NULL THEN TRUE ELSE FALSE END    AS IsUnlocked,

                pp.current_level                                        AS CurrentLevel,
                pp.total_points                                         AS TotalPoints,
                pp.total_missions                                       AS TotalMissions,

                CASE
                    WHEN pb.id IS NOT NULL THEN 100
                    WHEN b.badge_type = 'level' THEN
                        LEAST(100, FLOOR(
                            pp.total_points::numeric
                            / GREATEST(lreq.points_required, 1)
                            * 100
                        ))::int
                    WHEN b.badge_type = 'mission' THEN
                        LEAST(100, FLOOR(
                            pp.total_missions::numeric
                            / GREATEST(b.missions_required, 1)
                            * 100
                        ))::int
                    ELSE 0
                END                                                     AS ProgressPercentage

            FROM   public.badges b

            CROSS JOIN public.profile_progress pp

            LEFT JOIN public.profile_badges pb
                   ON pb.badge_id    = b.id
                  AND pb.profile_id  = @ProfileId

            LEFT JOIN public.levels lreq
                   ON lreq.level_number = b.level_required

            WHERE  pp.profile_id = @ProfileId
              AND  b.is_active   = TRUE

            ORDER BY b.badge_type, b.id
            """,
            new { ProfileId = profileId });
    }
}
