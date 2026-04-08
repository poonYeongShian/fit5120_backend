namespace Deploy.Models;

/// <summary>
/// Flat read model returned by the session-token validation join query (Flow 2).
/// Not persisted; used only to carry data from the repository to the service.
/// </summary>
public class ProfileAutoLoginRow
{
    public Guid ProfileId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int AnimalId { get; set; }
    public string ProfileCode { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int TotalPoints { get; set; }
    public int TotalQuizzes { get; set; }
    public int StreakDays { get; set; }
    public string LevelName { get; set; } = string.Empty;
}
