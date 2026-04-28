using Asp.Versioning;
using Asp.Versioning.Builder;
using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Deploy.Endpoints;

public static class TtsEndpoints
{
    public static void MapTtsEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/tts")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("TTS");

        group.MapPost("/generate", GenerateAudio)
            .WithName("GenerateTtsAudio")
            .WithDescription(
                "Converts text to speech using ElevenLabs with PostgreSQL cache. " +
                "If the same text/voice/model exists in cache, returns cached metadata without calling ElevenLabs.")
            .Produces<GenerateTtsAudioResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponseDto>(StatusCodes.Status500InternalServerError);

        group.MapGet("/audio/{textHash}", GetAudioByTextHash)
            .WithName("GetTtsAudioByTextHash")
            .WithDescription("Returns audio bytes for a cached/generated TTS record.")
            .Produces(StatusCodes.Status200OK, contentType: "audio/mpeg")
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<GenerateTtsAudioResponseDto>, BadRequest<ErrorResponseDto>, ProblemHttpResult>> GenerateAudio(
        GenerateTtsAudioRequestDto request,
        HttpContext httpContext,
        ITtsService service,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("TtsEndpoints");

        if (request is null || string.IsNullOrWhiteSpace(request.Text))
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_REQUEST",
                Details = new Dictionary<string, object?> { ["text"] = "Text is required." }
            });
        }

        try
        {
            logger.LogInformation(
                "Incoming TTS generate request. TextLength={TextLength}, VoiceId={VoiceId}, ModelId={ModelId}",
                request.Text?.Length ?? 0,
                request.VoiceId,
                request.ModelId);
            var audioUrlBase = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/v1/tts/audio";
            var response = await service.GenerateOrGetAudioAsync(request, audioUrlBase);
            return TypedResults.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorCode = "INVALID_REQUEST",
                Details = new Dictionary<string, object?> { ["message"] = ex.Message }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TTS generate failed for VoiceId={VoiceId}, ModelId={ModelId}", request.VoiceId, request.ModelId);
            return TypedResults.Problem(
                title: "TTS_GENERATION_FAILED",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<FileContentHttpResult, NotFound<ErrorResponseDto>>> GetAudioByTextHash(
        string textHash,
        ITtsService service,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("TtsEndpoints");
        logger.LogInformation("Incoming TTS audio request. TextHash={TextHash}", textHash);
        var audio = await service.GetAudioByTextHashAsync(textHash);
        if (audio is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "TTS_AUDIO_NOT_FOUND" });

        return TypedResults.File(audio.Value.AudioBytes, audio.Value.MimeType);
    }
}
