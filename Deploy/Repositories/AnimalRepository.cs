using Npgsql;
using Dapper;
using Deploy.Models;
using Deploy.Interfaces;

namespace Deploy.Repositories;

public class AnimalRepository : IAnimalRepository
{
    private readonly NpgsqlConnection _connection;

    public AnimalRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<Animal?> GetAnimalByIdAsync(int animalId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Animal>(
            @"SELECT id                      AS Id,
                     common_name             AS CommonName,
                     scientific_name         AS ScientificName,
                     category_id             AS CategoryId,
                     habitat                 AS Habitat,
                     diet                    AS Diet,
                     lifespan                AS Lifespan,
                     description             AS Description,
                     conservation_status_id  AS ConservationStatusId,
                     conservation_reason     AS ConservationReason,
                     image_url               AS ImageUrl,
                     created_at              AS CreatedAt,
                     updated_at              AS UpdatedAt
              FROM animals WHERE id = @Id",
            new { Id = animalId });
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Category>(
            @"SELECT id   AS Id,
                     name AS Name
              FROM categories WHERE id = @Id",
            new { Id = categoryId });
    }

    public async Task<ConservationStatus?> GetConservationStatusByIdAsync(int conservationStatusId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<ConservationStatus>(
            @"SELECT id              AS Id,
                     code           AS Code,
                     label          AS Label,
                     description    AS Description,
                     severity_order AS SeverityOrder
              FROM conservation_statuses WHERE id = @Id",
            new { Id = conservationStatusId });
    }

    public async Task<IEnumerable<(Animal Animal, Category? Category, ConservationStatus? ConservationStatus)>> GetAllAnimalsWithDetailsAsync(string? category = null)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        var sql = @"SELECT a.id                      AS Id,
                     a.common_name             AS CommonName,
                     a.scientific_name         AS ScientificName,
                     a.category_id             AS CategoryId,
                     a.habitat                 AS Habitat,
                     a.diet                    AS Diet,
                     a.lifespan                AS Lifespan,
                     a.conservation_status_id  AS ConservationStatusId,
                     a.conservation_reason     AS ConservationReason,
                     a.image_url               AS ImageUrl,
                     a.created_at              AS CreatedAt,
                     a.updated_at              AS UpdatedAt,
                     c.id                      AS Id,
                     c.name                    AS Name,
                     cs.id                     AS Id,
                     cs.code                   AS Code,
                     cs.label                  AS Label,
                     cs.description            AS Description,
                     cs.severity_order         AS SeverityOrder
              FROM animals a
              LEFT JOIN categories c             ON c.id = a.category_id
              LEFT JOIN conservation_statuses cs ON cs.id = a.conservation_status_id";

        if (!string.IsNullOrWhiteSpace(category))
            sql += "\n              WHERE LOWER(c.name) = LOWER(@Category)";

        sql += "\n              ORDER BY a.id";

        var rows = await connection.QueryAsync<Animal, Category, ConservationStatus, (Animal, Category?, ConservationStatus?)>(
            sql,
            (animal, cat, conservationStatus) => (animal, cat, conservationStatus),
            new { Category = category },
            splitOn: "Id,Id");

        return rows;
    }

    public async Task<IEnumerable<AnimalOccurrence>> GetOccurrencesByAnimalIdAsync(int animalId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<AnimalOccurrence>(
            @"SELECT id            AS Id,
                     animal_id     AS AnimalId,
                     latitude      AS Latitude,
                     longitude     AS Longitude,
                     location_name AS LocationName,
                     observed_at   AS ObservedAt,
                     notes         AS Notes,
                     created_at    AS CreatedAt
              FROM animal_occurrences
              WHERE animal_id = @AnimalId
              ORDER BY observed_at DESC",
            new { AnimalId = animalId });
    }
}
