using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace Deploy.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profiles")
            .WithTags("Profiles");

        group.MapPost("/", CreateProfile)
            .WithName("CreateProfile")
            .WithDescription(
                "Creates a new child profile with a unique profile code, initialises their " +
                "progress at level 1, and issues a session token for automatic device login.")
            .Produces<CreateProfileResponseDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Responses["201"].Description =
                    "Profile created successfully. Returns profile details and session token.";
                operation.Responses["400"].Description =
                    "Invalid request body. Error code: INVALID_REQUEST.";
                return operation;
            });

        group.MapGet("/", AutoLogin)
            .WithName("AutoLogin")
            .WithDescription(
                "Validates the session token supplied in the X-Session-Token header and returns " +
                "the profile together with current progress and level info. " +
                "Refreshes the session's last_used_at timestamp on every successful call.")
            .Produces<ProfileAutoLoginDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
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
                    "Session is valid. Returns profile, progress and level data.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or expired.";
                return operation;
            });

        group.MapDelete("/session", Logout)
            .WithName("Logout")
            .WithDescription(
                "Invalidates the session token supplied in the X-Session-Token header, " +
                "logging the device out. The token cannot be used again after this call.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Session-Token",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Session token to invalidate.",
                    Schema = new OpenApiSchema { Type = "string" }
                });

                operation.Responses["204"].Description = "Session invalidated successfully.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or already inactive.";
                return operation;
            });

        group.MapPost("/restore", RestoreProfile)
            .WithName("RestoreProfile")
            .WithDescription(
                "Validates a profile_code and PIN, then issues a new session token for the " +
                "requesting device. Use this for both account recovery (Flow 4) and signing " +
                "in on a new device (Flow 5).")
            .Produces<RestoreProfileResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description =
                    "Credentials valid. Returns profile, progress and a new session token.";
                operation.Responses["400"].Description =
                    "Missing or empty fields. Error code: INVALID_REQUEST.";
                operation.Responses["401"].Description =
                    "profile_code / PIN combination not found or profile inactive.";
                return operation;
            });

        group.MapPost("/progress", SaveProgress)
            .WithName("SaveProgress")
            .WithDescription(
                "Saves a completed quiz attempt for a profile. " +
                "Supply the session token via the X-Session-Token header. " +
                "Adds points, checks for a level-up, unlocks new fun facts, " +
                "and stamps the history row with the final level.")
            .Produces<SaveProgressResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
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
                    "Progress saved. Returns updated points, level info, level-up flag and newly unlocked fact count.";
                operation.Responses["400"].Description =
                    "Missing or invalid fields. Error code: INVALID_REQUEST.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid, expired or does not belong to this profile.";
                operation.Responses["404"].Description =
                    "Profile not found. Error code: PROFILE_NOT_FOUND.";
                return operation;
            });
    }

    private static async Task<Results<Created<CreateProfileResponseDto>, BadRequest<ErrorResponseDto>>> CreateProfile(
        CreateProfileRequestDto request,
        IProfileService service)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Pin)         ||
            request.AnimalId <= 0)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_REQUEST",
                Details = new Dictionary<string, object?>
                {
                    ["message"] = "DisplayName, Pin and a valid AnimalId are required."
                }
            });
        }

        var response = await service.CreateProfileAsync(request);
        return TypedResults.Created($"/api/profiles", response);
    }

    private static async Task<Results<Ok<ProfileAutoLoginDto>, UnauthorizedHttpResult>> AutoLogin(
        HttpContext httpContext,
        IProfileService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var result = await service.AutoLoginAsync(sessionToken);

        if (result is null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> Logout(
        HttpContext httpContext,
        IProfileService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var invalidated = await service.LogoutAsync(sessionToken);

        if (!invalidated)
            return TypedResults.Unauthorized();

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<RestoreProfileResponseDto>, BadRequest<ErrorResponseDto>, UnauthorizedHttpResult>> RestoreProfile(
        RestoreProfileRequestDto request,
        IProfileService service)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileCode) ||
            string.IsNullOrWhiteSpace(request.Pin))
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_REQUEST",
                Details = new Dictionary<string, object?>
                {
                    ["message"] = "ProfileCode and Pin are required."
                }
            });
        }

        var result = await service.RestoreProfileAsync(request);

        if (result is null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<SaveProgressResponseDto>, BadRequest<ErrorResponseDto>, NotFound<ErrorResponseDto>, UnauthorizedHttpResult>> SaveProgress(
        SaveProgressRequestDto request,
        HttpContext httpContext,
        IProfileService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await service.AutoLoginAsync(sessionToken);

        if (session is null || session.ProfileId != request.ProfileId)
            return TypedResults.Unauthorized();

        if (request.TotalQuestions <= 0)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_REQUEST",
                Details = new Dictionary<string, object?>
                {
                    ["message"] = "TotalQuestions is required and must be valid."
                }
            });
        }

        var result = await service.SaveProgressAsync(request);

        if (result is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "PROFILE_NOT_FOUND" });

        return TypedResults.Ok(result);
    }
}
