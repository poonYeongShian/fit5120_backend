namespace Deploy.DTOs;

public class CompleteMissionResponseDto
{
    public int TotalPoints { get; set; }
    public int NewLevel { get; set; }
    public bool LeveledUp { get; set; }
    public int NewFactsUnlocked { get; set; }
    public List<BadgeDto> NewBadges { get; set; } = [];
}

public class BadgeDto
{
    public string BadgeName { get; set; } = string.Empty;
    public string? BadgeImageUrl { get; set; }
    public string? Description { get; set; }
}
