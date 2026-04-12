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
                "Returns a list of animal cards. Optionally filter by category. " +
                "Available categories: Mammals, Birds, Reptiles, Amphibians, Marine & Freshwater, Insects.")
            .Produces<IEnumerable<AnimalCardDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                var categoryParam = operation.Parameters.FirstOrDefault(p => p.Name == "category");
                if (categoryParam is not null)
                {
                    categoryParam.Description =
                        "Filter by animal category. Available values: Mammals, Birds, Reptiles, Amphibians, Marine & Freshwater, Insects.";
                    categoryParam.Required = false;
                }

                operation.Responses["200"].Description = "A list of animal cards matching the optional category filter.";
                operation.Responses["400"].Description =
                    "Invalid category provided. Error code: INVALID_CATEGORY. " +
                    "Response includes 'availableCategories' in details.";

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
        string? category = null)
    {
        if (!string.IsNullOrWhiteSpace(category) &&
            !AnimalCategories.Available.Contains(category, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_CATEGORY",
                Details = new Dictionary<string, object?>
                {
                    ["availableCategories"] = AnimalCategories.Available
                }
            });
        }

        var animalCards = await service.GetAllAnimalCardsAsync(category);
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
