using Deploy.Interfaces;

namespace Deploy.Endpoints;

public static class AnimalEndpoints
{
    public static void MapAnimalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/animals");

        group.MapGet("/", GetAllAnimals)
            .WithName("GetAnimalList")
            .WithOpenApi();

        group.MapGet("/{animalId:int}", GetAnimalById)
            .WithName("GetAnimalCard")
            .WithOpenApi();

        group.MapGet("/{animalId:int}/occurrence", GetAnimalOccurrences)
            .WithName("GetAnimalOccurrences")
            .WithOpenApi();
    }

    private static async Task<IResult> GetAllAnimals(IAnimalService service)
    {
        var animalCards = await service.GetAllAnimalCardsAsync();
        return Results.Ok(animalCards);
    }

    private static async Task<IResult> GetAnimalById(int animalId, IAnimalService service)
    {
        var animalCard = await service.GetAnimalCardDetailAsync(animalId);

        if (animalCard is null)
            return Results.NotFound(new { message = "Animal not found" });

        return Results.Ok(animalCard);
    }

    private static async Task<IResult> GetAnimalOccurrences(int animalId, IAnimalService service)
    {
        var occurrences = await service.GetAnimalOccurrencesAsync(animalId);

        if (occurrences is null)
            return Results.NotFound(new { message = "Animal not found" });

        return Results.Ok(occurrences);
    }
}
