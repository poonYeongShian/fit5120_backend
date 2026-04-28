namespace Deploy.DTOs;

public class GenerateTtsAudioRequestDto
{
    public string Text { get; set; } = string.Empty;
    public string? VoiceId { get; set; }
    public string? ModelId { get; set; }
}
