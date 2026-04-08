namespace Deploy.Models;

/// <summary>
/// Flat read model returned after the level-up UPDATE in Step 3 (Save Progress flow).
/// Carries the resolved current_level, total_points and level metadata back to the service.
/// </summary>
public class ProgressAfterQuizRow
{
    public int CurrentLevel { get; set; }
    public int TotalPoints { get; set; }
    public string LevelName { get; set; } = string.Empty;
}
