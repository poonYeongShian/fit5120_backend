namespace Deploy.DTOs;

public class BadgeCollectionDto
{
    public int Id { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string? BadgeImageUrl { get; set; }
    public string? Description { get; set; }
    public string BadgeType { get; set; } = string.Empty;
    public int? LevelRequired { get; set; }
    public int? MissionsRequired { get; set; }
    public string? Source { get; set; }
    public DateTimeOffset? EarnedAt { get; set; }
    public bool IsUnlocked { get; set; }
    public int CurrentLevel { get; set; }
    public int TotalPoints { get; set; }
    public int TotalMissions { get; set; }
    public int ProgressPercentage { get; set; }
}
