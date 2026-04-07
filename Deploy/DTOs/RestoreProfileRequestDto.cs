namespace Deploy.DTOs;

public class RestoreProfileRequestDto
{
    public string ProfileCode { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
}
