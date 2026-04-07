namespace Deploy.Models;

public class ProfileSession
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset LastUsedAt { get; set; }
}
