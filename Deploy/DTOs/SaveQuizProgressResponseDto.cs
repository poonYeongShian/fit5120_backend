namespace Deploy.DTOs;

public class SaveQuizProgressResponseDto
{
    public int TotalPoints { get; set; }
    public int NewLevel { get; set; }
    public bool LeveledUp { get; set; }
    public int NewFactsUnlocked { get; set; }
    public List<BadgeDto> NewBadges { get; set; } = [];
}
