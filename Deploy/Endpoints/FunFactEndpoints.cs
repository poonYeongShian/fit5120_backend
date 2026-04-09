using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace Deploy.Endpoints;

public static class FunFactEndpoints
{
    public static void MapFunFactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fun-facts")
            .WithTags("Fun Facts");

        group.MapGet("/", GetAllFunFacts)
            .WithName("GetAllFunFacts")
            .WithDescription(
                "Returns all fun facts across all animals with personalised lock/unlock status " +
                "based on the requesting profile's current level. " +
                "Supply the session token via the X-Session-Token header.")
            .Produces<IEnumerable<AnimalFunFactDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Session-Token",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Session token issued during profile creation or restore.",
                    Schema = new OpenApiSchema { Type = "string" }
                });

                operation.Responses["200"].Description =
                    "Fun facts with access_status ('locked'/'unlocked'), levels_needed and profile level info.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid, expired or does not belong to this profile.";
                operation.Responses["404"].Description =
                    "Profile not found. Error code: PROFILE_NOT_FOUND.";
                return operation;
            });

        group.MapGet("/{animalId:int}", GetFunFacts)
            .WithName("GetAnimalFunFacts")
            .WithDescription(
                "Returns all fun facts for an animal with personalised lock/unlock status " +
                "based on the requesting profile's current level. " +
                "Supply the session token via the X-Session-Token header.")
            .Produces<IEnumerable<AnimalFunFactDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Session-Token",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Session token issued during profile creation or restore.",
                    Schema = new OpenApiSchema { Type = "string" }
                });

                operation.Responses["200"].Description =
                    "Fun facts with access_status ('locked'/'unlocked'), levels_needed and profile level info.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid, expired or does not belong to this profile.";
                operation.Responses["404"].Description =
                    "Profile not found. Error code: PROFILE_NOT_FOUND.";
                return operation;
            });


    }


    private static async Task<Results<Ok<IEnumerable<AnimalFunFactDto>>, UnauthorizedHttpResult, NotFound<ErrorResponseDto>>> GetFunFacts(
        int animalId,
        HttpContext httpContext,
        IProfileService profileService,
        IFunFactService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var facts = await service.GetFunFactsByAnimalAsync(animalId, session.ProfileId);

        if (facts is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "PROFILE_NOT_FOUND" });

        return TypedResults.Ok(facts);
    }

    private static async Task<Results<Ok<IEnumerable<AnimalFunFactDto>>, UnauthorizedHttpResult, NotFound<ErrorResponseDto>>> GetAllFunFacts(
        HttpContext httpContext,
        IProfileService profileService,
        IFunFactService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var facts = await service.GetAllFunFactsAsync(session.ProfileId);

        if (facts is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "PROFILE_NOT_FOUND" });

        return TypedResults.Ok(facts);
    }
}
