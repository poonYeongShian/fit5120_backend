using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

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
                "Pass the WMO weather code and whether it is daytime to receive an appropriate indoor or outdoor mission.")
            .Produces<WeatherMissionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                var weatherCodeParam = operation.Parameters.FirstOrDefault(p => p.Name == "weatherCode");
                if (weatherCodeParam is not null)
                {
                    weatherCodeParam.Description =
                        "WMO weather code from the weather API (e.g. 0 = Clear, 80 = Rain Showers, 95 = Thunderstorm).";
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
                "Assigns a specific mission to a profile by creating a profile_missions record with status 'assigned'. " +
                "Pass the mission Id obtained from GET /weather-adaptive along with optional weather/location snapshot data. " +
                "Returns the profileMissionId which should be passed to POST /start and POST /complete.")
            .Produces<AssignMissionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description =
                    "Mission assigned successfully. Returns the profileMissionId to use in subsequent /start and /complete calls.";
                operation.Responses["400"].Description =
                    "Failed to assign mission. The mission or profile may not exist. Error code: ASSIGN_FAILED.";

                return operation;
            });

        group.MapPost("/start", StartMission)
            .WithName("StartMission")
            .WithDescription(
                "Marks an assigned mission as in-progress by updating the profile_missions record. " +
                "Pass the profileMissionId obtained from POST /assign. Sets status to 'in_progress' and records started_at timestamp.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description =
                    "Mission started successfully. Status updated to in_progress.";
                operation.Responses["404"].Description =
                    "No matching assigned mission found for this profileMissionId, or mission was already started/completed. Error code: MISSION_NOT_FOUND.";

                return operation;
            });

        group.MapPost("/complete", CompleteMission)
            .WithName("CompleteMission")
            .WithDescription(
                "Marks a mission as completed and updates progress: " +
                "awards points, checks for level-up, unlocks new fun facts, and awards any earned badges. " +
                "Pass the profileMissionId obtained from POST /assign.")
            .Produces<CompleteMissionResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description =
                    "Mission completed successfully. Returns updated progress, level info, and any newly earned badges.";
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

    private static async Task<Results<Ok<AssignMissionResponseDto>, BadRequest<ErrorResponseDto>>> AssignMission(
        AssignMissionRequestDto request,
        IMissionService service)
    {
        var profileMissionId = await service.AssignMissionAsync(
            request.ProfileId, request.MissionId, request.WeatherCode, request.IsDay,
            request.WeatherTemp, request.LocationLat, request.LocationLon);

        if (profileMissionId is null)
            return TypedResults.BadRequest(new ErrorResponseDto { ErrorCode = "ASSIGN_FAILED" });

        return TypedResults.Ok(new AssignMissionResponseDto { ProfileMissionId = profileMissionId.Value });
    }

    private static async Task<Results<Ok, NotFound<ErrorResponseDto>>> StartMission(
        StartMissionRequestDto request,
        IMissionService service)
    {
        var success = await service.StartMissionAsync(request.ProfileMissionId);

        if (!success)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "MISSION_NOT_FOUND" });

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<CompleteMissionResponseDto>, NotFound<ErrorResponseDto>>> CompleteMission(
        CompleteMissionRequestDto request,
        IMissionService service)
    {
        var result = await service.CompleteMissionAsync(request.ProfileMissionId);

        if (result is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "MISSION_NOT_FOUND" });

        return TypedResults.Ok(result);
    }
}
