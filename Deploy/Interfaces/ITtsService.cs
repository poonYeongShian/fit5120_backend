using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface ITtsService
{
    Task<GenerateTtsAudioResponseDto> GenerateOrGetAudioAsync(GenerateTtsAudioRequestDto request, string audioUrlBase);
    Task<(byte[] AudioBytes, string MimeType)?> GetAudioByTextHashAsync(string textHash);
}
