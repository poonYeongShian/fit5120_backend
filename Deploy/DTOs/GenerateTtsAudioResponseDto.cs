namespace Deploy.DTOs;

public class GenerateTtsAudioResponseDto
{
    public string TextHash { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public decimal Speed { get; set; } = 1.0m;
    public string MimeType { get; set; } = "audio/mpeg";
    public bool Cached { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public IReadOnlyList<TtsWordTimingDto> Timings { get; set; } = Array.Empty<TtsWordTimingDto>();
}
