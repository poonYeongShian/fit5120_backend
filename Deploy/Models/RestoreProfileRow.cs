namespace Deploy.Models;

/// <summary>
/// Flat read model returned by the profile_code + PIN validation join query (Flow 4 & 5).
/// Not persisted; used only to carry data from the repository to the service.
/// </summary>
public class RestoreProfileRow
{
    public Guid ProfileId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProfileCode { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int TotalPoints { get; set; }
    public int StreakDays { get; set; }
}
