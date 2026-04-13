namespace Deploy.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
