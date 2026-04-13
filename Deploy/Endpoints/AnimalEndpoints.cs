using Asp.Versioning;
using Asp.Versioning.Builder;
using Deploy.Constants;
using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Deploy.Endpoints;

public static class AnimalEndpoints
{
    public static void MapAnimalEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/animals")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Animals");

        group.MapGet("/", GetAllAnimals)
            .WithName("GetAnimalList")
            .WithDescription(
                "Returns a list of animal cards. Optionally filter by animal class. " +
                "Available classes: Mammals, Birds, Reptiles, Amphibians, Marine & Freshwater, Insects.")
            .Produces<IEnumerable<AnimalCardDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                var classParam = operation.Parameters.FirstOrDefault(p => p.Name == "animalClass");
                if (classParam is not null)
                {
                    classParam.Description =
                        "Filter by animal class. Available values: Mammals, Birds, Reptiles, Amphibians, Marine & Freshwater, Insects.";
                    classParam.Required = false;
                }

                operation.Responses["200"].Description = "A list of animal cards matching the optional class filter.";
                operation.Responses["400"].Description =
                    "Invalid animal class provided. Error code: INVALID_ANIMAL_CLASS. " +
                    "Response includes 'availableClasses' in details.";

                return operation;
            });

        group.MapGet("/{animalId:int}", GetAnimalById)
            .WithName("GetAnimalCard")
            .WithDescription("Returns detailed information for a specific animal by its ID.")
            .Produces<AnimalCardDetailDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description = "The detailed animal card for the given ID.";
                operation.Responses["404"].Description =
                    "Animal not found. Error code: ANIMAL_NOT_FOUND.";

                return operation;
            });

        group.MapGet("/{animalId:int}/occurrence", GetAnimalOccurrences)
            .WithName("GetAnimalOccurrences")
            .WithDescription("Returns occurrence/sighting records for a specific animal by its ID.")
            .Produces<IEnumerable<AnimalOccurrenceDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description = "A list of occurrence records for the given animal.";
                operation.Responses["404"].Description =
                    "Animal not found. Error code: ANIMAL_NOT_FOUND.";

                return operation;
            });
    }

    private static async Task<Results<Ok<IEnumerable<AnimalCardDto>>, BadRequest<ErrorResponseDto>>> GetAllAnimals(
        IAnimalService service,
        string? animalClass = null)
    {
        if (!string.IsNullOrWhiteSpace(animalClass) &&
            !AnimalClasses.Available.Contains(animalClass, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_ANIMAL_CLASS",
                Details = new Dictionary<string, object?>
                {
                    ["availableClasses"] = AnimalClasses.Available
                }
            });
        }

        var animalCards = await service.GetAllAnimalCardsAsync(animalClass);
        return TypedResults.Ok(animalCards);
    }

    private static async Task<Results<Ok<AnimalCardDetailDto>, NotFound<ErrorResponseDto>>> GetAnimalById(int animalId, IAnimalService service)
    {
        var animalCard = await service.GetAnimalCardDetailAsync(animalId);

        if (animalCard is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "ANIMAL_NOT_FOUND" });

        return TypedResults.Ok(animalCard);
    }

    private static async Task<Results<Ok<IEnumerable<AnimalOccurrenceDto>>, NotFound<ErrorResponseDto>>> GetAnimalOccurrences(int animalId, IAnimalService service)
    {
        var occurrences = await service.GetAnimalOccurrencesAsync(animalId);

        if (occurrences is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "ANIMAL_NOT_FOUND" });

        return TypedResults.Ok(occurrences);
    }
}
