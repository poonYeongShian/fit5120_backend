using Dapper;
using Deploy.Interfaces;
using Deploy.Models;
using Npgsql;

namespace Deploy.Repositories;

public class FunFactRepository : IFunFactRepository
{
    private readonly NpgsqlConnection _connection;

    public FunFactRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<AnimalFunFact>> GetFunFactsByAnimalIdAsync(int animalId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<AnimalFunFact>(
            """
            SELECT f.animal_fun_fact_id AS Id,
                   f.animal_id          AS AnimalId,
                   f.emoji              AS Emoji,
                   f.fact_text          AS FactText,
                   f.fact_image_url     AS FactImageUrl,
                   f.fact_order         AS FactOrder,
                   f.level_id           AS LevelId,
                   l.level_number       AS UnlockLevelNumber,
                   f.created_at         AS CreatedAt
            FROM   public.animal_fun_fact f
            JOIN   public.level l ON l.level_id = f.level_id
            WHERE  f.animal_id = @AnimalId
            ORDER BY f.fact_order
            """,
            new { AnimalId = animalId });
    }

    public async Task<IEnumerable<AnimalFunFact>> GetAllFunFactsAsync()
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<AnimalFunFact>(
            """
            SELECT f.animal_fun_fact_id AS Id,
                   f.animal_id          AS AnimalId,
                   f.emoji              AS Emoji,
                   f.fact_text          AS FactText,
                   f.fact_image_url     AS FactImageUrl,
                   f.fact_order         AS FactOrder,
                   f.level_id           AS LevelId,
                   l.level_number       AS UnlockLevelNumber,
                   f.created_at         AS CreatedAt
            FROM   public.animal_fun_fact f
            JOIN   public.level l ON l.level_id = f.level_id
            ORDER BY f.fact_order
            """);
    }

    public async Task<IEnumerable<int>> GetUnlockedFactIdsByProfileAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<int>(
            """
            SELECT animal_fun_fact_id
            FROM   public.profile_unlocked_fact
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId });
    }
}
