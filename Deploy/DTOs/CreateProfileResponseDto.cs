namespace Deploy.DTOs;

public class CreateProfileResponseDto
{
    public Guid ProfileId { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
}
