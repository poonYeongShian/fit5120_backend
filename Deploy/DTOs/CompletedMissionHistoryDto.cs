namespace Deploy.DTOs;

public class CompletedMissionHistoryDto
{
    public int MissionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public bool IsOutdoor { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}
