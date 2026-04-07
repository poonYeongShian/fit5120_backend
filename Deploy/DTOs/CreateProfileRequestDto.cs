namespace Deploy.DTOs;

public class CreateProfileRequestDto
{
    public string DisplayName { get; set; } = string.Empty;
    public int AnimalId { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
}
