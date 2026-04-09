using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace Deploy.Endpoints;

public static class MissionEndpoints
{
    public static void MapMissionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/missions")
            .WithTags("Missions");

        group.MapGet("/weather-adaptive", GetWeatherAdaptiveMission)
            .WithName("GetWeatherAdaptiveMission")
            .WithDescription(
                "Returns a single random mission adapted to the current weather conditions and time of day. " +
                "Pass the WMO weather code and whether it is daytime to receive an appropriate indoor or outdoor mission.\n\n" +
                "### Supported WMO Weather Codes\n\n" +
                "| WMO Min | WMO Max | Condition         |\n" +
                "|---------|---------|-------------------|\n" +
                "| 0       | 0       | Clear Sky         |\n" +
                "| 1       | 3       | Mainly Clear      |\n" +
                "| 45      | 48      | Foggy             |\n" +
                "| 51      | 55      | Drizzle           |\n" +
                "| 61      | 65      | Rain              |\n" +
                "| 71      | 77      | Snowfall          |\n" +
                "| 80      | 82      | Rain Showers      |\n" +
                "| 95      | 99      | Thunderstorm      |\n")
            .Produces<WeatherMissionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                var weatherCodeParam = operation.Parameters.FirstOrDefault(p => p.Name == "weatherCode");
                if (weatherCodeParam is not null)
                {
                    weatherCodeParam.Description =
                        "WMO weather code from the weather API. Supported ranges: " +
                        "0 = Clear Sky, " +
                        "1�3 = Mainly Clear, " +
                        "45�48 = Foggy, " +
                        "51�55 = Drizzle, " +
                        "61�65 = Rain, " +
                        "71�77 = Snowfall, " +
                        "80�82 = Rain Showers, " +
                        "95�99 = Thunderstorm.";
                    weatherCodeParam.Required = true;
                }

                var isDayParam = operation.Parameters.FirstOrDefault(p => p.Name == "isDay");
                if (isDayParam is not null)
                {
                    isDayParam.Description =
                        "Whether it is currently daytime (true) or night (false). Maps to is_day from the weather API.";
                    isDayParam.Required = true;
                }

                operation.Responses["200"].Description = "A random mission matching the weather and time-of-day conditions.";
                operation.Responses["404"].Description =
                    "No matching mission found for the given weather code and time of day. Error code: MISSION_NOT_FOUND.";

                return operation;
            });

        group.MapPost("/assign", AssignMission)
            .WithName("AssignMission")
            .WithDescription(
                "Assigns a specific mission to the authenticated profile by creating a profile_missions record with status 'assigned'. " +
                "Pass the mission Id obtained from GET /weather-adaptive along with optional weather/location snapshot data. " +
                "Supply the session token via the X-Session-Token header. " +
                "Returns the profileMissionId which should be passed to POST /start and POST /complete.")
            .Produces<AssignMissionResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
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
                    "Mission assigned successfully. Returns the profileMissionId to use in subsequent /start and /complete calls.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or expired.";
                operation.Responses["400"].Description =
                    "Failed to assign mission. The mission or profile may not exist. Error code: ASSIGN_FAILED.";

                return operation;
            });

        group.MapPost("/start", StartMission)
            .WithName("StartMission")
            .WithDescription(
                "Marks an assigned mission as in-progress by updating the profile_missions record. " +
                "Pass the profileMissionId obtained from POST /assign. " +
                "Supply the session token via the X-Session-Token header. " +
                "Sets status to 'in_progress' and records started_at timestamp.")
            .Produces(StatusCodes.Status200OK)
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
                    "Mission started successfully. Status updated to in_progress.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or expired.";
                operation.Responses["404"].Description =
                    "No matching assigned mission found for this profileMissionId, or mission was already started/completed. Error code: MISSION_NOT_FOUND.";

                return operation;
            });

        group.MapGet("/history", GetCompletedMissionHistory)
            .WithName("GetCompletedMissionHistory")
            .WithDescription(
                "Returns the last 3 missions completed by the authenticated profile, " +
                "ordered by completion date descending. " +
                "Supply the session token via the X-Session-Token header.")
            .Produces<List<CompletedMissionHistoryDto>>(StatusCodes.Status200OK)
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
                    "A list of the last 3 completed missions (may be empty if none completed yet).";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or expired.";

                return operation;
            });

        group.MapPost("/complete", CompleteMission)
            .WithName("CompleteMission")
            .WithDescription(
                "Marks a mission as completed and updates progress: " +
                "awards points, checks for level-up, unlocks new fun facts, and awards any earned badges. " +
                "Pass the profileMissionId obtained from POST /assign. " +
                "Supply the session token via the X-Session-Token header.")
            .Produces<CompleteMissionResponseDto>(StatusCodes.Status200OK)
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
                    "Mission completed successfully. Returns updated progress, level info, and any newly earned badges.";
                operation.Responses["401"].Description =
                    "Session token missing, invalid or expired.";
                operation.Responses["404"].Description =
                    "No matching active mission found for this profileMissionId, or mission was already completed. Error code: MISSION_NOT_FOUND.";

                return operation;
            });
    }

    private static async Task<Results<Ok<WeatherMissionDto>, NotFound<ErrorResponseDto>>> GetWeatherAdaptiveMission(
        int weatherCode,
        bool isDay,
        IMissionService service)
    {
        var mission = await service.GetWeatherAdaptiveMissionAsync(weatherCode, isDay);

        if (mission is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "MISSION_NOT_FOUND" });

        return TypedResults.Ok(mission);
    }

    private static async Task<Results<Ok<AssignMissionResponseDto>, UnauthorizedHttpResult, BadRequest<ErrorResponseDto>>> AssignMission(
        AssignMissionRequestDto request,
        HttpContext httpContext,
        IProfileService profileService,
        IMissionService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var profileMissionId = await service.AssignMissionAsync(
            session.ProfileId, request.MissionId, request.WeatherCode, request.IsDay,
            request.WeatherTemp, request.LocationLat, request.LocationLon);

        if (profileMissionId is null)
            return TypedResults.BadRequest(new ErrorResponseDto { ErrorCode = "ASSIGN_FAILED" });

        return TypedResults.Ok(new AssignMissionResponseDto { ProfileMissionId = profileMissionId.Value });
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult, NotFound<ErrorResponseDto>>> StartMission(
        StartMissionRequestDto request,
        HttpContext httpContext,
        IProfileService profileService,
        IMissionService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var success = await service.StartMissionAsync(request.ProfileMissionId);

        if (!success)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "MISSION_NOT_FOUND" });

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<List<CompletedMissionHistoryDto>>, UnauthorizedHttpResult>> GetCompletedMissionHistory(
        HttpContext httpContext,
        IProfileService profileService,
        IMissionService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var history = await service.GetCompletedMissionHistoryAsync(session.ProfileId);

        return TypedResults.Ok(history);
    }

    private static async Task<Results<Ok<CompleteMissionResponseDto>, UnauthorizedHttpResult, NotFound<ErrorResponseDto>>> CompleteMission(
        CompleteMissionRequestDto request,
        HttpContext httpContext,
        IProfileService profileService,
        IMissionService service)
    {
        var sessionToken = httpContext.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(sessionToken))
            return TypedResults.Unauthorized();

        var session = await profileService.AutoLoginAsync(sessionToken);

        if (session is null)
            return TypedResults.Unauthorized();

        var result = await service.CompleteMissionAsync(request.ProfileMissionId);

        if (result is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "MISSION_NOT_FOUND" });

        return TypedResults.Ok(result);
    }
}
