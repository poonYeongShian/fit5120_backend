namespace Deploy.DTOs;

public class SaveProgressResponseDto
{
    public int HistoryId { get; set; }
    public int TotalPoints { get; set; }
    public int CurrentLevel { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public string? BadgeEmoji { get; set; }
    public bool LeveledUp { get; set; }
    public int LevelBefore { get; set; }
    public int LevelAfter { get; set; }
    public int NewFactsUnlocked { get; set; }
}
