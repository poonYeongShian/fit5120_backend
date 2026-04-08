namespace Deploy.DTOs;

public class RestoreProfileResponseDto
{
    public Guid ProfileId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int AnimalId { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int TotalPoints { get; set; }
    public int StreakDays { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
}
