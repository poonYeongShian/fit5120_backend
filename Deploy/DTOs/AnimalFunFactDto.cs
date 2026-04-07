namespace Deploy.DTOs;

public class AnimalFunFactDto
{
    public int Id { get; set; }
    public string? Emoji { get; set; }
    public string FactText { get; set; } = string.Empty;
    public string? FactImageUrl { get; set; }
    public int FactOrder { get; set; }
    public int UnlockLevel { get; set; }
    public bool IsLocked { get; set; }
    public string AccessStatus { get; set; } = string.Empty;   // "locked" | "unlocked"
    public int LevelsNeeded { get; set; }
    public int UserLevel { get; set; }
    public int UserPoints { get; set; }
    public string UserLevelName { get; set; } = string.Empty;
    public string? BadgeEmoji { get; set; }
    public bool AlreadyUnlocked { get; set; }
}
