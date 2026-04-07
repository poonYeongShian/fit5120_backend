namespace Deploy.Models;

public class Level
{
    public int Id { get; set; }
    public int LevelNumber { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int PointsRequired { get; set; }
    public string? BadgeEmoji { get; set; }
    public string? BadgeColor { get; set; }
}
