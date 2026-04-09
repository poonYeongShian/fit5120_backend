using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace Deploy.Endpoints;

public static class BadgeEndpoints
{
    public static void MapBadgeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/badges")
            .WithTags("Badges");

        group.MapGet("/", GetBadgeCollection)
            .WithName("GetBadgeCollection")
            .WithDescription(
                "Returns all badges (earned and not yet earned) for the authenticated profile, " +
                "including unlock status, progress percentage, and criteria to unlock each badge. " +
                "Supply the session token via the X-Session-Token header.")
            .Produces<IEnumerable<BadgeCollectionDto>>(StatusCodes.Status200OK)
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
                    "All badges with unlock status, progress percentage, and criteria.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid, expired or does not belong to this profile.";
                operation.Responses["404"].Description =
                    "Profile not found. Error code: PROFILE_NOT_FOUND.";
                return operation;
            });
    }

    private static async Task<Results<Ok<IEnumerable<BadgeCollectionDto>>, UnauthorizedHttpResult, NotFound<ErrorResponseDto>>> GetBadgeCollection(
        HttpContext httpContext,
        IProfileService profileService,
        IBadgeService badgeService)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var badges = await badgeService.GetBadgeCollectionAsync(session.ProfileId);

        if (badges is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "PROFILE_NOT_FOUND" });

        return TypedResults.Ok(badges);
    }
}
