namespace Deploy.Models;

public class TtsCacheEntry
{
    public int Id { get; set; }
    public string TextHash { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string MimeType { get; set; } = "audio/mpeg";
    public byte[] AudioContent { get; set; } = Array.Empty<byte>();
    public string TimingsJson { get; set; } = "[]";
}
