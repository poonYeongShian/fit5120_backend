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
            SELECT id            AS Id,
                   animal_id     AS AnimalId,
                   emoji         AS Emoji,
                   fact_text     AS FactText,
                   fact_image_url AS FactImageUrl,
                   fact_order    AS FactOrder,
                   unlock_level  AS UnlockLevel,
                   is_locked     AS IsLocked,
                   created_at    AS CreatedAt
            FROM   public.animal_fun_facts
            WHERE  animal_id = @AnimalId
            ORDER BY fact_order
            """,
            new { AnimalId = animalId });
    }

    public async Task<IEnumerable<AnimalFunFact>> GetAllFunFactsAsync()
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<AnimalFunFact>(
            """
            SELECT id            AS Id,
                   animal_id     AS AnimalId,
                   emoji         AS Emoji,
                   fact_text     AS FactText,
                   fact_image_url AS FactImageUrl,
                   fact_order    AS FactOrder,
                   unlock_level  AS UnlockLevel,
                   is_locked     AS IsLocked,
                   created_at    AS CreatedAt
            FROM   public.animal_fun_facts
            ORDER BY fact_order
            """);
    }

    public async Task<IEnumerable<int>> GetUnlockedFactIdsByProfileAsync(Guid profileId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<int>(
            """
            SELECT fact_id
            FROM   public.profile_unlocked_facts
            WHERE  profile_id = @ProfileId
            """,
            new { ProfileId = profileId });
    }
}
